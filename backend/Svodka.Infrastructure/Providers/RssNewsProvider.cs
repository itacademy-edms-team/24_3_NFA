using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Services;

namespace Svodka.Infrastructure.Providers
{
    /// <summary>
    /// Провайдер для получения новостей из RSS-лент
    /// </summary>
    public class RssNewsProvider : INewsProvider
    {
        private readonly IRssService _rssService;
        private readonly ILogger<RssNewsProvider> _logger;

        public RssNewsProvider(IRssService rssService, ILogger<RssNewsProvider> logger)
        {
            _rssService = rssService;
            _logger = logger;
        }

        public SourceType Type => SourceType.Rss;

        public async Task<IEnumerable<NewsItem>> GetNewsAsync(object configuration)
        {
            if (configuration is not RssSourceConfiguration rssConfig)
            {
                _logger.LogError("Неправильный тип конфигурации для RssNewsProvider: {Type}", configuration?.GetType().FullName);
                throw new ArgumentException("Ожидается RssSourceConfiguration.", nameof(configuration));
            }

            _logger.LogInformation("Загрузка новостей из RSS: {Url}", rssConfig.Url);

            try
            {
                var items = await _rssService.FetchRssFeedAsync(rssConfig.Url, rssConfig.Limit);
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении новостей из RSS: {Url}", rssConfig.Url);
                throw;
            }
        }

        public string ValidateAndNormalize(JsonElement json)
        {
            var rssConfig = JsonSerializer.Deserialize<RssSourceConfiguration>(json.GetRawText());
            if (rssConfig == null) throw new ArgumentException("Invalid RSS configuration.");

            var normalizedUrl = rssConfig.Url?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(normalizedUrl)) throw new ArgumentException("The provided URL is empty.");

            if (!normalizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                normalizedUrl = "https://" + normalizedUrl;
            }

            if (!Uri.IsWellFormedUriString(normalizedUrl, UriKind.Absolute))
            {
                throw new ArgumentException("The provided URL is not valid.");
            }

            rssConfig.Url = normalizedUrl;
            return JsonSerializer.Serialize(rssConfig);
        }

        public object DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<RssSourceConfiguration>(json) 
                ?? throw new ArgumentException("Failed to deserialize RSS configuration.");
            
            if (config.Limit == 0)
            {
                config.Limit = defaultLimit;
            }
            return config;
        }

        public string? GetCategory(string json)
        {
            var config = JsonSerializer.Deserialize<RssSourceConfiguration>(json);
            return config?.Category;
        }
    }
}
