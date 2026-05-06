using System.Text.Json;
using System.Text.Json.Serialization;
using Svodka.Domain.Interfaces;

namespace Svodka.Domain.Models
{
    public class GitHubSourceConfiguration : ISourceConfiguration
    {
        public string RepositoryOwner { get; set; } = string.Empty;
        public string RepositoryName { get; set; } = string.Empty;
        public string? Token { get; set; }
        public List<string>? EventTypes { get; set; }
        public int Limit { get; set; } = 10;
        public string? Category { get; set; }

        public static GitHubSourceConfiguration FromJson(JsonElement json) =>
            SourceConfigurationJson.Deserialize<GitHubSourceConfiguration>(json);

        public GitHubSourceConfiguration Normalize()
        {
            RepositoryOwner = RepositoryOwner?.Trim() ?? string.Empty;
            RepositoryName = RepositoryName?.Trim() ?? string.Empty;
            Token = string.IsNullOrWhiteSpace(Token) ? null : Token.Trim();
            if (Limit <= 0) Limit = 10;
            return this;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(RepositoryOwner))
            {
                throw new ArgumentException("Укажите владельца репозитория GitHub.");
            }
            if (string.IsNullOrWhiteSpace(RepositoryName))
            {
                throw new ArgumentException("Укажите название репозитория GitHub.");
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
