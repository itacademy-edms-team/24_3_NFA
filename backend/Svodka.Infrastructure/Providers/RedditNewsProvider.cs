using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Services;

namespace Svodka.Infrastructure.Providers
{
    /// <summary>
    /// Провайдер для получения новостей из Reddit
    /// </summary>
    public class RedditNewsProvider : INewsProvider
    {
        private readonly IRedditService _redditService;
        private readonly ILogger<RedditNewsProvider> _logger;

        public RedditNewsProvider(IRedditService redditService, ILogger<RedditNewsProvider> logger)
        {
            _redditService = redditService;
            _logger = logger;
        }

        public SourceType Type => SourceType.Reddit;

        public async Task<IEnumerable<NewsItem>> GetNewsAsync(object configuration)
        {
            if (configuration is not RedditSourceConfiguration redditConfig)
            {
                _logger.LogError("Неправильный тип конфигурации для RedditNewsProvider: {Type}", configuration?.GetType().FullName);
                throw new ArgumentException("Ожидается RedditSourceConfiguration.", nameof(configuration));
            }

            _logger.LogInformation("Загрузка новостей из Reddit: r/{Subreddit}", redditConfig.Subreddit);

            try
            {
                var items = await _redditService.FetchSubredditPostsAsync(
                    redditConfig.Subreddit, 
                    redditConfig.SortType ?? "hot", 
                    redditConfig.Limit);
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении новостей из Reddit: r/{Subreddit}", redditConfig.Subreddit);
                throw;
            }
        }

        public string ValidateAndNormalize(JsonElement json)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var redditConfig = JsonSerializer.Deserialize<RedditSourceConfiguration>(json.GetRawText(), options);

            if (redditConfig == null || string.IsNullOrWhiteSpace(redditConfig.Subreddit))
            {
                throw new ArgumentException("Reddit configuration must include Subreddit.");
            }

            var validSortTypes = new[] { "hot", "new", "top" };
            if (!validSortTypes.Contains(redditConfig.SortType?.ToLower() ?? ""))
            {
                redditConfig.SortType = "hot";
            }

            return JsonSerializer.Serialize(redditConfig);
        }

        public object DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<RedditSourceConfiguration>(json)
                ?? throw new ArgumentException("Failed to deserialize Reddit configuration.");

            if (config.Limit == 0)
            {
                config.Limit = defaultLimit;
            }
            return config;
        }

        public string? GetCategory(string json)
        {
            var config = JsonSerializer.Deserialize<RedditSourceConfiguration>(json);
            return config?.Category;
        }
    }
}
