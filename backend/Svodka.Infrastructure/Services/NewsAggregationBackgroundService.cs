using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Data;

namespace Svodka.Infrastructure.Services
{
    public class NewsAggregationOptions
    {
        public const string SectionName = "NewsAggregation";
        public int PollingIntervalMinutes { get; set; } = 5; 
        public int NewsLimitPerSource { get; set; } = 10; 
    }

    public class NewsAggregationBackgroundService : BackgroundService
    {
        private readonly ILogger<NewsAggregationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider; 
        private readonly NewsAggregationOptions _options;

        public NewsAggregationBackgroundService(
            ILogger<NewsAggregationBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<NewsAggregationOptions> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Служба агрегации новостей запущена. Интервал: {Interval} минут", _options.PollingIntervalMinutes);

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.PollingIntervalMinutes));

            try
            {
                await DoWorkAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWorkAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Служба агрегации новостей остановлена.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка в фоновой службе агрегации.");
            }
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Начало цикла агрегации...");

            using var scope = _serviceProvider.CreateScope();

            var providerFactory = scope.ServiceProvider.GetRequiredService<INewsProviderFactory>();
            var sourceRepository = scope.ServiceProvider.GetRequiredService<INewsSourceRepository>();
            var itemRepository = scope.ServiceProvider.GetRequiredService<INewsItemRepository>();
            var dbContext = scope.ServiceProvider.GetRequiredService<NewsAggregatorDbContext>(); 

            try
            {
                var activeSources = await sourceRepository.GetActiveNewsSourcesAsync();
                _logger.LogDebug("Найдено {Count} активных источников.", activeSources.Count());

                foreach (var source in activeSources)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    try
                    {
                        _logger.LogDebug("Обработка источника: {SourceName} (ID: {SourceId})", source.Name, source.Id);
                        if (source.Type.Equals("rss", StringComparison.OrdinalIgnoreCase))
                        {
                            var configObject = System.Text.Json.JsonSerializer.Deserialize<RssSourceConfiguration>(source.Configuration);
                            if (configObject.Limit == 0) configObject.Limit = _options.NewsLimitPerSource;

                            var provider = providerFactory.GetProvider(source.Type);

                            var newsItems = await provider.GetNewsAsync(configObject);

                            var itemsWithSourceId = newsItems.Select(item =>
                            {
                                item.SourceId = source.Id;
                                return item;
                            });

                            await itemRepository.SaveNewsAsync(itemsWithSourceId);

                            await sourceRepository.UpdateLastPolledAtAsync(source.Id, DateTime.UtcNow);

                            _logger.LogDebug("Обработано {Count} новостей из источника: {SourceName}", itemsWithSourceId.Count(), source.Name);
                        }
                        else
                        {
                            _logger.LogWarning("Неизвестный тип источника: {SourceType} для ID {SourceId}", source.Type, source.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при обработке источника {SourceName} (ID: {SourceId}): {ErrorMessage}", source.Name, source.Id, ex.Message);

                        await sourceRepository.UpdateLastErrorAsync(source.Id, DateTime.UtcNow, ex.Message);
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogDebug("Все изменения за цикл сохранены в БД.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении цикла агрегации.");
                throw; 
            }

            _logger.LogDebug("Цикл агрегации завершён.");
        }
    }
}