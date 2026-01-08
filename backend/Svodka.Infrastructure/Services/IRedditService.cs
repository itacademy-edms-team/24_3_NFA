using Svodka.Domain.Entities;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Интерфейс для работы с Reddit JSON API
    /// </summary>
    public interface IRedditService
    {
        /// <summary>
        /// Получает посты из сабреддита через Reddit JSON API
        /// </summary>
        /// <param name="subreddit">Название сабреддита (без префикса r/)</param>
        /// <param name="sortType">Тип сортировки: hot, new, top</param>
        /// <param name="limit">Лимит постов</param>
        /// <returns>Список новостей из постов Reddit</returns>
        Task<IEnumerable<NewsItem>> FetchSubredditPostsAsync(string subreddit, string sortType, int limit);
    }
}

