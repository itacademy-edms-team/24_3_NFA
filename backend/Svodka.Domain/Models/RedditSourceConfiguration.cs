using System.Text.Json;
using System.Text.Json.Serialization;
using Svodka.Domain.Interfaces;

namespace Svodka.Domain.Models
{
    public class RedditSourceConfiguration : ISourceConfiguration
    {
        public string Subreddit { get; set; } = string.Empty;
        public string SortType { get; set; } = "hot";
        public int Limit { get; set; } = 10;
        public string? Category { get; set; }

        public static RedditSourceConfiguration FromJson(JsonElement json) =>
            SourceConfigurationJson.Deserialize<RedditSourceConfiguration>(json);

        public RedditSourceConfiguration Normalize()
        {
            Subreddit = Subreddit?.Trim().TrimStart('r', '/').TrimStart('/') ?? string.Empty;
            SortType = (SortType ?? "hot").Trim().ToLowerInvariant();
            var validSort = new[] { "hot", "new", "top" };
            if (!validSort.Contains(SortType))
            {
                SortType = "hot";
            }
            if (Limit <= 0) Limit = 10;
            return this;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Subreddit))
            {
                throw new ArgumentException("Укажите название сабреддита Reddit.");
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(Subreddit, @"^[A-Za-z0-9_]+$"))
            {
                throw new ArgumentException("Некорректное название сабреддита Reddit.");
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
