using System.Text.Json.Serialization;

namespace Svodka.Domain.Entities
{
    /// <summary>
    /// Промежуточная сущность связи источника с тегом
    /// </summary>
    public class NewsSourceTag
    {
        /// <summary>
        /// Идентификатор источника
        /// </summary>
        public int NewsSourceId { get; set; }

        /// <summary>
        /// Идентификатор тега
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// Источник новости
        /// </summary>
        [JsonIgnore]
        public virtual NewsSource NewsSource { get; set; } = null!;

        /// <summary>
        /// Тег
        /// </summary>
        public virtual Tag Tag { get; set; } = null!;
    }
}
