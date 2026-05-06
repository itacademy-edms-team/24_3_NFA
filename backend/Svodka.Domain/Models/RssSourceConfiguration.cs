using System.Text.Json;
using System.Text.Json.Serialization;
using Svodka.Domain.Interfaces;

namespace Svodka.Domain.Models
{
    public class RssSourceConfiguration : ISourceConfiguration
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        public static RssSourceConfiguration FromJson(JsonElement json) =>
            SourceConfigurationJson.Deserialize<RssSourceConfiguration>(json);

        public RssSourceConfiguration Normalize()
        {
            Url = Url?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(Url) &&
                !Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Url = "https://" + Url;
            }
            if (Limit <= 0) Limit = 10;
            return this;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                throw new ArgumentException("Укажите URL RSS-ленты.");
            }
            if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                throw new ArgumentException("Указан некорректный URL RSS-ленты.");
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
