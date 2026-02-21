using System.Text.Json.Serialization;

namespace Svodka.Domain.Entities
{
    /// <summary>
    /// Сущность, представляющая собой новость
    /// </summary>
    public class NewsItem
    {
        /// <summary>
        /// Уникальный идентификатор новости
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Заголовок новости
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Описание или краткое содержание новости
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Ссылка на новость
        /// </summary>
        public string Link { get; set; } = string.Empty;

        /// <summary>
        /// Дата и время публикации новости в формате UTC
        /// </summary>
        public DateTime PublishedAtUtc { get; set; }

        /// <summary>
        /// Идентификатор источника, из которого была получена новость
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// Уникальный идентификатор новости в источнике
        /// </summary>
        public string SourceItemId { get; set; } = string.Empty;

        /// <summary>
        /// Автор новости (необязательное поле)
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// URL изображения новости (необязательное поле)
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Категория новости (необязательное поле)
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Дата и время индексации новости в системе
        /// </summary>
        public DateTime IndexedAtUtc { get; set; }

        /// <summary>
        /// Ссылка на источник новости (внешний ключ, игнорируется при сериализации JSON)
        /// </summary>
        [JsonIgnore]
        public virtual NewsSource? NewsSource { get; set; }

        /// <summary>
        /// Тип источника новости (вычисляемое свойство для сериализации)
        /// </summary>
        public string? SourceType => NewsSource?.Type;

        /// <summary>
        /// Дополнительные данные источника в формате JSON (например, тип события GitHub, SHA коммита и т.д.)
        /// </summary>
        public string? Metadata { get; set; }
    }
}