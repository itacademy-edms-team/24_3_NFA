namespace Svodka.Domain.Models
{
    /// <summary>
    /// Конфигурация GitHub-источника
    /// </summary>
    public class GitHubSourceConfiguration
    {
        /// <summary>
        /// Владелец репозитория (username или organization)
        /// </summary>
        public string RepositoryOwner { get; set; } = string.Empty;

        /// <summary>
        /// Название репозитория
        /// </summary>
        public string RepositoryName { get; set; } = string.Empty;

        /// <summary>
        /// GitHub Personal Access Token (опционально, для приватных репозиториев или увеличения лимита запросов)
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Типы событий для отслеживания (push, issues, pull_request и т.д.). Если пусто, отслеживаются все события
        /// </summary>
        public List<string>? EventTypes { get; set; }

        /// <summary>
        /// Лимит событий для получения
        /// </summary>
        public int Limit { get; set; } = 10;
    }
}

