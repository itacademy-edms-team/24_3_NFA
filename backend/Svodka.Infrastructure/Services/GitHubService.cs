using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Сервис для работы с GitHub API
    /// </summary>
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GitHubService> _logger;
        private const string GitHubApiBaseUrl = "https://api.github.com";

        public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Svodka News Aggregator 1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            _logger = logger;
        }

        public async Task<IEnumerable<NewsItem>> FetchRepositoryEventsAsync(
            string owner, 
            string repo, 
            string? token, 
            int limit,
            List<string>? eventTypes = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new AuthenticationHeaderValue("Bearer", token);
                }

                var url = $"{GitHubApiBaseUrl}/repos/{owner}/{repo}/events?per_page={Math.Min(limit, 100)}";
                _logger.LogInformation("Загрузка событий GitHub репозитория: {Owner}/{Repo}", owner, repo);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var events = JsonSerializer.Deserialize<List<GitHubEvent>>(jsonString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<GitHubEvent>();


                if (eventTypes != null && eventTypes.Any())
                {
                    events = events.Where(e => eventTypes.Contains(e.Type, StringComparer.OrdinalIgnoreCase)).ToList();
                }


                events = events.Take(limit).ToList();

                var newsItems = events.Select(e => ConvertEventToNewsItem(e, owner, repo)).ToList();

                _logger.LogInformation("Получено {Count} событий из репозитория {Owner}/{Repo}", newsItems.Count, owner, repo);

                return newsItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке событий GitHub репозитория {Owner}/{Repo}", owner, repo);
                throw;
            }
            finally
            {
                // Очищаем заголовок авторизации после использования
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        private NewsItem ConvertEventToNewsItem(GitHubEvent gitHubEvent, string owner, string repo)
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

        private string GetEventTitle(GitHubEvent gitHubEvent)
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

        private string GetEventDescription(GitHubEvent gitHubEvent)
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

