using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Models
{
    /// <summary>
    /// Конфигурация RSS-источника
    /// </summary>
    using System.Text.Json.Serialization;

    public class RssSourceConfiguration
    {
        /// <summary>
        /// URL RSS-ленты
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Максимальное количество новостей для получения
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;

        /// <summary>
        /// Категория источника (необязательное поле)
        /// </summary>
        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }

}
