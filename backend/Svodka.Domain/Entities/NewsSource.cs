using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Svodka.Domain.Enums;

namespace Svodka.Domain.Entities
{
    /// <summary>
    /// Представляет собой источник новостей
    /// </summary>
    public class NewsSource
    {
        /// <summary>
        /// Уникальный идентификатор источника
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название источника новостей
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип источника
        /// </summary>
        public SourceType Type {  get; set; }

        /// <summary>
        /// Строка конфигурации в формате JSON
        /// </summary>
        public string Configuration { get; set; } = string.Empty;

        /// <summary>
        /// Флаг активности источника (активный/неактивный)
        /// </summary>
        public bool IsActive {  get; set; }

        /// <summary>
        /// Дата и время последней проверки источника
        /// </summary>
        public DateTime? LastPolledAtUtc { get; set; }

        /// <summary>
        /// Дата и время последней ошибки при обработке источника
        /// </summary>
        public DateTime? LastErrorAtUtc { get; set; }

        /// <summary>
        /// Текст последней ошибки при обработке источника
        /// </summary>
        public string? LastError { get; set; }

        /// <summary>
        /// Идентификатор пользователя-владельца
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Пользователь-владелец источника
        /// </summary>
        [JsonIgnore]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Коллекция новостей, полученных из этого источника
        /// </summary>
        [JsonIgnore]
        public virtual ICollection<NewsItem> NewsItems { get; set; } = new List<NewsItem>();

    }
}
