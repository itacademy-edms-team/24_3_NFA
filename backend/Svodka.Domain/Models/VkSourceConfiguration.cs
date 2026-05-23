using System.Text.Json;
using System.Text.Json.Serialization;
using Svodka.Domain.Interfaces;

namespace Svodka.Domain.Models
{
    public class VkSourceConfiguration : ISourceConfiguration
    {
        /// <summary>Короткий адрес: habr, club123, durov (из ссылки vk.com/...).</summary>
        [JsonPropertyName("domain")]
        public string? Domain { get; set; }

        /// <summary>owner_id: отрицательный для сообществ (-123), положительный для пользователя.</summary>
        [JsonPropertyName("ownerId")]
        public long? OwnerId { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;

        /// <summary>all (по умолчанию), owner, others, suggests, postponed, donut.</summary>
        [JsonPropertyName("filter")]
        public string Filter { get; set; } = "all";

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        public static VkSourceConfiguration FromJson(JsonElement json) =>
            SourceConfigurationJson.Deserialize<VkSourceConfiguration>(json);

        public VkSourceConfiguration Normalize()
        {
            Domain = Domain?.Trim().ToLowerInvariant() ?? string.Empty;
            Domain = Domain
                .Replace("https://", "")
                .Replace("http://", "")
                .Replace("vk.com/", "")
                .Replace("www.", "")
                .Trim('/');
            Filter = (Filter ?? "all").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(Filter)) Filter = "all";
            if (Limit <= 0) Limit = 10;
            return this;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Domain) && !OwnerId.HasValue)
            {
                throw new ArgumentException("Укажите короткий адрес VK (domain) или числовой owner_id.");
            }
            var allowedFilters = new[] { "all", "owner", "others", "postponed", "suggests", "donut" };
            if (!allowedFilters.Contains(Filter, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException("filter VK должен быть: all, owner, others, postponed, suggests или donut.");
            }
            SourceConfigurationJson.ValidateLimit(Limit);
        }

        public string ToJson() => SourceConfigurationJson.Serialize(this);

        public static string ValidateAndNormalizeFromJson(JsonElement json)
        {
            var config = FromJson(json).Normalize();
            config.Validate();
            return config.ToJson();
        }
    }
}
