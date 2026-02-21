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
                if (gitHubEvent.Type == "PushEvent" && gitHubEvent.Payload.Commits != null && gitHubEvent.Payload.Commits.Any())
                {
                    var firstCommit = gitHubEvent.Payload.Commits.First();
                    sha = firstCommit.Sha;
                    link = $"https://github.com/{owner}/{repo}/commit/{sha}";
                    description = firstCommit.Message + "\n\n" + description;
                }
                else if (gitHubEvent.Type == "IssuesEvent" && gitHubEvent.Payload.Issue != null)
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
                "PushEvent" => $"{actor} pushed {gitHubEvent.Payload?.Commits?.Count ?? 0} commit(s) to {repo}",
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
            public string Id { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string CreatedAtRaw { get; set; } = string.Empty;
            
            [JsonIgnore]
            public DateTime CreatedAt 
            {
                get
                {
                    if (DateTime.TryParse(CreatedAtRaw, out var result))
                    {
                        return result;
                    }
                    return DateTime.UtcNow;
                }
            }
            
            public GitHubActor? Actor { get; set; }
            public GitHubRepo? Repo { get; set; }
            public GitHubPayload? Payload { get; set; }
        }

        private class GitHubActor
        {
            public string Login { get; set; } = string.Empty;
            public string? AvatarUrl { get; set; }
        }

        private class GitHubRepo
        {
            public string Name { get; set; } = string.Empty;
        }

        private class GitHubPayload
        {
            public List<GitHubCommit>? Commits { get; set; }
            public GitHubIssue? Issue { get; set; }
            public GitHubPullRequest? PullRequest { get; set; }
            public string? RefType { get; set; }
            public GitHubRelease? Release { get; set; }
        }

        private class GitHubCommit
        {
            public string Sha { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        private class GitHubIssue
        {
            public string Title { get; set; } = string.Empty;
            public string? Body { get; set; }
            public string? HtmlUrl { get; set; }
        }

        private class GitHubPullRequest
        {
            public string Title { get; set; } = string.Empty;
            public string? Body { get; set; }
            public string? HtmlUrl { get; set; }
            public int Number { get; set; }
        }

        private class GitHubRelease
        {
            public string Name { get; set; } = string.Empty;
            public string? Body { get; set; }
            public string? HtmlUrl { get; set; }
        }
    }
}

