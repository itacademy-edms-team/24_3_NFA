using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;

namespace Svodka.Infrastructure.Fetchers
{
    public class GitHubNewsSourceFetcher : INewsSourceFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubNewsSourceFetcher> _logger;
        private const string GitHubApiBaseUrl = "https://api.github.com";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public GitHubNewsSourceFetcher(HttpClient httpClient, ILogger<GitHubNewsSourceFetcher> logger)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Svodka News Aggregator 1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            _logger = logger;
        }

        public SourceType Type => SourceType.GitHub;

        public async Task<IEnumerable<NewsItem>> FetchAsync(
            NewsSource source,
            int defaultLimit,
            CancellationToken ct = default)
        {
            var config = DeserializeConfiguration(source.Configuration, defaultLimit);

            try
            {
                if (!string.IsNullOrEmpty(config.Token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", config.Token);
                }

                var url = $"{GitHubApiBaseUrl}/repos/{config.RepositoryOwner}/{config.RepositoryName}/events?per_page={Math.Min(config.Limit, 100)}";
                _logger.LogInformation(
                    "Загрузка событий GitHub репозитория: {Owner}/{Repo}",
                    config.RepositoryOwner,
                    config.RepositoryName);

                var response = await _httpClient.GetAsync(url, ct);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException(
                        $"Репозиторий {config.RepositoryOwner}/{config.RepositoryName} не найден на GitHub.");
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new InvalidOperationException(
                        "Доступ к GitHub API запрещён. Проверьте токен или лимит запросов.");
                }
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync(ct);
                var events = JsonSerializer.Deserialize<List<GitHubEvent>>(jsonString, JsonOptions)
                    ?? new List<GitHubEvent>();

                if (config.EventTypes != null && config.EventTypes.Any())
                {
                    events = events
                        .Where(e => config.EventTypes.Contains(e.Type, StringComparer.OrdinalIgnoreCase))
                        .ToList();
                }

                var newsItems = events
                    .Take(config.Limit)
                    .Select(e => ConvertEventToNewsItem(e, config.RepositoryOwner, config.RepositoryName))
                    .ToList();

                _logger.LogInformation(
                    "Получено {Count} событий из репозитория {Owner}/{Repo}",
                    newsItems.Count,
                    config.RepositoryOwner,
                    config.RepositoryName);

                return newsItems;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при загрузке событий GitHub репозитория {Owner}/{Repo}",
                    config.RepositoryOwner,
                    config.RepositoryName);
                throw new InvalidOperationException(
                    $"Не удалось загрузить события GitHub для {config.RepositoryOwner}/{config.RepositoryName}.",
                    ex);
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public string ValidateAndNormalize(JsonElement json) =>
            GitHubSourceConfiguration.ValidateAndNormalizeFromJson(json);

        public IEnumerable<string> GetSuggestedTags(string json)
        {
            var config = JsonSerializer.Deserialize<GitHubSourceConfiguration>(json, JsonOptions);
            var tag = config?.Category ?? "GitHub";
            return new List<string> { tag };
        }

        private static GitHubSourceConfiguration DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<GitHubSourceConfiguration>(json, SourceConfigurationJson.Options)
                ?? throw new ArgumentException("Некорректная конфигурация GitHub.");

            if (config.Limit == 0)
            {
                config.Limit = defaultLimit;
            }

            return config;
        }

        private static NewsItem ConvertEventToNewsItem(GitHubEvent gitHubEvent, string owner, string repo)
        {
            var title = GetEventTitle(gitHubEvent);
            var description = GetEventDescription(gitHubEvent);
            var link = $"https://github.com/{owner}/{repo}";
            string? sha = null;
            int? prNumber = null;

            if (gitHubEvent.Payload != null)
            {
                if (gitHubEvent.Type == "IssuesEvent" && gitHubEvent.Payload.Issue != null)
                {
                    link = gitHubEvent.Payload.Issue.HtmlUrl ?? link;
                }
                else if (gitHubEvent.Type == "PullRequestEvent" && gitHubEvent.Payload.PullRequest != null)
                {
                    var pr = gitHubEvent.Payload.PullRequest;
                    link = pr.HtmlUrl ?? link;
                    prNumber = pr.Number;
                }
                else if (gitHubEvent.Type == "ReleaseEvent" && gitHubEvent.Payload.Release != null)
                {
                    link = gitHubEvent.Payload.Release.HtmlUrl ?? $"https://github.com/{owner}/{repo}/releases";
                }
            }

            var metadata = new
            {
                gitHubType = gitHubEvent.Type,
                sha,
                prNumber
            };

            return new NewsItem
            {
                Title = title,
                Description = description,
                Link = link,
                PublishedAtUtc = gitHubEvent.CreatedAt,
                SourceItemId = gitHubEvent.Id,
                Author = gitHubEvent.Actor?.Login,
                ImageUrl = gitHubEvent.Actor?.AvatarUrl,
                Category = gitHubEvent.Type,
                IndexedAtUtc = DateTime.UtcNow,
                Metadata = JsonSerializer.Serialize(metadata)
            };
        }

        private static string GetEventTitle(GitHubEvent gitHubEvent)
        {
            return gitHubEvent.Type switch
            {
                "PushEvent" => $"Push to {gitHubEvent.Repo?.Name ?? "repository"}",
                "IssuesEvent" => $"Issue: {gitHubEvent.Payload?.Issue?.Title ?? "Unknown"}",
                "PullRequestEvent" => $"Pull Request: {gitHubEvent.Payload?.PullRequest?.Title ?? "Unknown"}",
                "CreateEvent" => $"Created {gitHubEvent.Payload?.RefType ?? "resource"} in {gitHubEvent.Repo?.Name ?? "repository"}",
                "DeleteEvent" => $"Deleted {gitHubEvent.Payload?.RefType ?? "resource"} from {gitHubEvent.Repo?.Name ?? "repository"}",
                "ReleaseEvent" => $"Release: {gitHubEvent.Payload?.Release?.Name ?? "Unknown"}",
                _ => $"{gitHubEvent.Type} in {gitHubEvent.Repo?.Name ?? "repository"}"
            };
        }

        private static string GetEventDescription(GitHubEvent gitHubEvent)
        {
            var actor = gitHubEvent.Actor?.Login ?? "Unknown";
            var repo = gitHubEvent.Repo?.Name ?? "repository";

            return gitHubEvent.Type switch
            {
                "PushEvent" => $"{actor} pushed to {repo}",
                "IssuesEvent" => gitHubEvent.Payload?.Issue?.Body ?? $"Issue event in {repo}",
                "PullRequestEvent" => gitHubEvent.Payload?.PullRequest?.Body ?? $"Pull request event in {repo}",
                "CreateEvent" => $"{actor} created {gitHubEvent.Payload?.RefType ?? "resource"} in {repo}",
                "DeleteEvent" => $"{actor} deleted {gitHubEvent.Payload?.RefType ?? "resource"} from {repo}",
                "ReleaseEvent" => gitHubEvent.Payload?.Release?.Body ?? $"Release in {repo}",
                _ => $"{actor} performed {gitHubEvent.Type} in {repo}"
            };
        }

        private class GitHubEvent
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [JsonPropertyName("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonPropertyName("actor")]
            public GitHubActor? Actor { get; set; }

            [JsonPropertyName("repo")]
            public GitHubRepo? Repo { get; set; }

            [JsonPropertyName("payload")]
            public GitHubPayload? Payload { get; set; }
        }

        private class GitHubActor
        {
            [JsonPropertyName("login")]
            public string Login { get; set; } = string.Empty;

            [JsonPropertyName("avatar_url")]
            public string? AvatarUrl { get; set; }
        }

        private class GitHubRepo
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }

        private class GitHubPayload
        {
            [JsonPropertyName("issue")]
            public GitHubIssue? Issue { get; set; }

            [JsonPropertyName("pull_request")]
            public GitHubPullRequest? PullRequest { get; set; }

            [JsonPropertyName("ref_type")]
            public string? RefType { get; set; }

            [JsonPropertyName("release")]
            public GitHubRelease? Release { get; set; }
        }

        private class GitHubIssue
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("body")]
            public string? Body { get; set; }

            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }
        }

        private class GitHubPullRequest
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;

            [JsonPropertyName("body")]
            public string? Body { get; set; }

            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }

            [JsonPropertyName("number")]
            public int Number { get; set; }
        }

        private class GitHubRelease
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("body")]
            public string? Body { get; set; }

            [JsonPropertyName("html_url")]
            public string? HtmlUrl { get; set; }
        }
    }
}
