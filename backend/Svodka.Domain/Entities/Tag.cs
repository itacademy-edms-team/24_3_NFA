using System.Text.Json.Serialization;

namespace Svodka.Domain.Entities
{
    /// <summary>
    /// Тег, который может быть назначен нескольким источникам
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Уникальный идентификатор тега
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Отображаемое имя тега
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Нормализованное имя тега для поиска и дедупликации
        /// </summary>
        public string NormalizedName { get; set; } = string.Empty;

        /// <summary>
        /// Связи тега с источниками
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<NewsSourceTag> NewsSourceTags { get; set; } = new List<NewsSourceTag>();
    }
}
