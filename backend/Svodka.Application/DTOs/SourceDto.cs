using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Svodka.Domain.Enums;

namespace Svodka.Application.DTOs
{
    /// <summary>
    /// DTO для создания или обновления источника новостей
    /// </summary>
    public class SourceDto
    {
        /// <summary>
        /// Название источника
        /// </summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип источника
        /// </summary>
        [Required]
        public SourceType Type { get; set; }

        /// <summary>
        /// Конфигурация источника (может быть RssSourceConfiguration, GitHubSourceConfiguration или RedditSourceConfiguration)
        /// </summary>
        [Required]
        public JsonElement Configuration { get; set; }

        /// <summary>
        /// Флаг активности источника
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
