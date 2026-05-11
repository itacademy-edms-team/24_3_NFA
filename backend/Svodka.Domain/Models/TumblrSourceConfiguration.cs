using System.Text.Json;
using System.Text.Json.Serialization;
using Svodka.Domain.Interfaces;

namespace Svodka.Domain.Models
{
    public class TumblrSourceConfiguration : ISourceConfiguration
    {
        [JsonPropertyName("blogName")]
        public string BlogName { get; set; } = string.Empty;

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 10;

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        public string RssUrl => $"https://{BlogName}.tumblr.com/rss";

        public string BlogIdentifier => $"{BlogName}.tumblr.com";

        public static TumblrSourceConfiguration FromJson(JsonElement json) =>
            SourceConfigurationJson.Deserialize<TumblrSourceConfiguration>(json);

        public TumblrSourceConfiguration Normalize()
        {
            BlogName = BlogName?.Trim().ToLowerInvariant() ?? string.Empty;
            BlogName = BlogName.Replace("https://", "").Replace("http://", "");
            if (BlogName.Contains(".tumblr.com"))
            {
                BlogName = BlogName.Split('.')[0];
            }
            if (Limit <= 0) Limit = 10;
            return this;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BlogName))
            {
                throw new ArgumentException("Укажите имя блога Tumblr.");
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(BlogName, @"^[a-z0-9\-]+$"))
            {
                throw new ArgumentException("Некорректное имя блога Tumblr.");
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
