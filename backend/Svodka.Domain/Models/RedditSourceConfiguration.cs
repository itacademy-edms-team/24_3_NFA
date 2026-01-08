namespace Svodka.Domain.Models
{
    /// <summary>
    /// Конфигурация Reddit-источника
    /// </summary>
    public class RedditSourceConfiguration
    {
        /// <summary>
        /// Название сабреддита (без префикса r/)
        /// </summary>
        public string Subreddit { get; set; } = string.Empty;

        /// <summary>
        /// Тип сортировки: hot, new, top
        /// </summary>
        public string SortType { get; set; } = "hot";

        /// <summary>
        /// Лимит постов для получения (максимум 100)
        /// </summary>
        public int Limit { get; set; } = 10;

        /// <summary>
        /// Категория для новостей из этого источника (опционально)
        /// </summary>
        public string? Category { get; set; }
    }
}
