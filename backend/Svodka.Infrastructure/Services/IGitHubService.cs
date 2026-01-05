using Svodka.Domain.Entities;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Интерфейс для работы с GitHub API
    /// </summary>
    public interface IGitHubService
    {
        /// <summary>
        /// Получает события репозитория через GitHub Events API
        /// </summary>
        /// <param name="owner">Владелец репозитория</param>
        /// <param name="repo">Название репозитория</param>
        /// <param name="token">GitHub Personal Access Token (опционально)</param>
        /// <param name="limit">Лимит событий</param>
        /// <param name="eventTypes">Типы событий для фильтрации (опционально)</param>
        /// <returns>Список новостей из событий репозитория</returns>
        Task<IEnumerable<NewsItem>> FetchRepositoryEventsAsync(
            string owner, 
            string repo, 
            string? token, 
            int limit,
            List<string>? eventTypes = null);
    }
}

