using System.Text.Json.Serialization;
using Svodka.Domain.Interfaces;

namespace Svodka.Domain.Models
{
    /// <summary>
    /// Конфигурация RSS-источника
    /// </summary>
    public class RssSourceConfiguration : ISourceConfiguration
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
