using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Services;

namespace Svodka.Infrastructure.Providers
{
    /// <summary>
    /// Провайдер для получения новостей из Reddit сабреддитов
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

        public async Task<IEnumerable<NewsItem>> GetNewsAsync(object configuration)
        {
            if (configuration is not RedditSourceConfiguration redditConfig)
            {
                _logger.LogError(
                    "Неправильный тип конфигурации для RedditNewsProvider: {Type}", 
                    configuration?.GetType().FullName);
                throw new ArgumentException(
                    "Ожидается RedditSourceConfiguration.", 
                    nameof(configuration));
            }

            _logger.LogInformation(
                "Загрузка новостей из Reddit: r/{Subreddit}", 
                redditConfig.Subreddit);

            try
            {
                var items = await _redditService.FetchSubredditPostsAsync(
                    redditConfig.Subreddit,
                    redditConfig.SortType,
                    redditConfig.Limit);

                // Применяем категорию, если указана в конфигурации
                var itemsWithCategory = items.Select(item =>
                {
                    if (!string.IsNullOrEmpty(redditConfig.Category))
                    {
                        item.Category = redditConfig.Category;
                    }
                    return item;
                }).ToList();

                return itemsWithCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Ошибка при получении новостей из Reddit: r/{Subreddit}", 
                    redditConfig.Subreddit);
                throw;
            }
        }
    }
}
