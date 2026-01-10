using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Data;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Служба для агрегации новостей из источников
    /// </summary>
    public class NewsAggregationJob : INewsAggregationJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NewsAggregationJob> _logger;
        private readonly NewsAggregationOptions _options;

        /// <summary>
        /// Конструктор службы агрегации новостей
        /// </summary>
        /// <param name="scopeFactory">Фабрика сервисных областей</param>
        /// <param name="logger">Логгер</param>
        /// <param name="options">Опции агрегации</param>
        public NewsAggregationJob(
            IServiceScopeFactory scopeFactory,
            ILogger<NewsAggregationJob> logger,
            IOptions<NewsAggregationOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Выполняет асинхронную агрегацию новостей
        /// </summary>
        /// <param name="sourceId">Идентификатор источника для агрегации (опционально, если null - обрабатываются все активные источники)</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача выполнения агрегации</returns>
        public async Task ExecuteAsync(int? sourceId = null, CancellationToken cancellationToken = default)
        {
            using var scope = _scopeFactory.CreateScope();

            var providerFactory = scope.ServiceProvider.GetRequiredService<INewsProviderFactory>();
            var sourceRepository = scope.ServiceProvider.GetRequiredService<INewsSourceRepository>();
            var itemRepository = scope.ServiceProvider.GetRequiredService<INewsItemRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<NewsAggregatorDbContext>();

            IEnumerable<NewsSource> sources;

            if (sourceId.HasValue)
            {
                var source = await sourceRepository.GetByIdAsync(sourceId.Value);
                if (source == null)
                {
                    _logger.LogWarning("Источник с ID {SourceId} не найден для агрегации.", sourceId);
                    return;
                }

                if (!source.IsActive)
                {
                    _logger.LogInformation("Источник {SourceId} не активен, агрегация пропущена.", sourceId);
                    return;
                }

                sources = new[] { source };
            }
            else
            {
                sources = await sourceRepository.GetActiveNewsSourcesAsync();
            }

            _logger.LogDebug("Запуск агрегации. Источников к обработке: {Count}", sources.Count());

            foreach (var source in sources)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    _logger.LogDebug("Обработка источника: {SourceName} (ID: {SourceId}, Type: {SourceType})", source.Name, source.Id, source.Type);

                    object? configObject = null;
                    var sourceType = source.Type.ToLower();

                    switch (sourceType)
                    {
                        case "rss":
                            configObject = JsonSerializer.Deserialize<RssSourceConfiguration>(source.Configuration);
                            if (configObject is RssSourceConfiguration rssConfig && rssConfig.Limit == 0)
                            {
                                rssConfig.Limit = _options.NewsLimitPerSource;
                            }
                            break;

                        case "github":
                            configObject = JsonSerializer.Deserialize<GitHubSourceConfiguration>(source.Configuration);
                            if (configObject is GitHubSourceConfiguration githubConfig && githubConfig.Limit == 0)
                            {
                                githubConfig.Limit = _options.NewsLimitPerSource;
                            }
                            break;

                        case "reddit":
                            configObject = JsonSerializer.Deserialize<RedditSourceConfiguration>(source.Configuration);
                            if (configObject is RedditSourceConfiguration redditConfig && redditConfig.Limit == 0)
                            {
                                redditConfig.Limit = _options.NewsLimitPerSource;
                            }
                            break;

                        default:
                            _logger.LogWarning("Неизвестный тип источника: {SourceType} для ID {SourceId}", source.Type, source.Id);
                            continue;
                    }

                    if (configObject == null)
                    {
                        _logger.LogWarning("Не удалось десериализовать конфигурацию источника {SourceId}", source.Id);
                        continue;
                    }

                    var provider = providerFactory.GetProvider(source.Type);

                    var newsItems = await provider.GetNewsAsync(configObject);

                    var itemsWithSourceId = newsItems.Select(item =>
                    {
                        item.SourceId = source.Id;
                        return item;
                    }).ToList();

                    if (itemsWithSourceId.Count == 0)
                    {
                        _logger.LogDebug("Источник {SourceName} не вернул новых новостей.", source.Name);
                        continue;
                    }

                    await itemRepository.SaveNewsAsync(itemsWithSourceId);

                    await sourceRepository.UpdateLastPolledAtAsync(source.Id, DateTime.UtcNow);

                    _logger.LogDebug("Обработано {Count} новостей из источника: {SourceName}", itemsWithSourceId.Count, source.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке источника {SourceName} (ID: {SourceId})", source.Name, source.Id);
                    await sourceRepository.UpdateLastErrorAsync(source.Id, DateTime.UtcNow, ex.Message);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Цикл агрегации завершён.");
        }
    }
}
