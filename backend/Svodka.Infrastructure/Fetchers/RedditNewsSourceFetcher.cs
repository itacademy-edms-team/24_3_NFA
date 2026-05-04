using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;

namespace Svodka.Infrastructure.Fetchers
{
    public class RedditNewsSourceFetcher : INewsSourceFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RedditNewsSourceFetcher> _logger;
        private const string RedditApiBaseUrl = "https://www.reddit.com";
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RedditNewsSourceFetcher(HttpClient httpClient, ILogger<RedditNewsSourceFetcher> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SvodkaApp/1.0 (Student Project)");
        }

        public SourceType Type => SourceType.Reddit;

        public async Task<IEnumerable<NewsItem>> FetchAsync(
            NewsSource source,
            int defaultLimit,
            CancellationToken ct = default)
        {
            var config = DeserializeConfiguration(source.Configuration, defaultLimit);

            try
            {
                var sortType = config.SortType ?? "hot";
                var timeFilter = sortType.ToLower() == "top" ? "&t=week" : string.Empty;
                var url = $"{RedditApiBaseUrl}/r/{config.Subreddit}/{sortType}.json?limit={Math.Min(config.Limit, 100)}{timeFilter}";

                _logger.LogInformation(
                    "Загрузка постов из Reddit: r/{Subreddit} (sort: {SortType}, limit: {Limit})",
                    config.Subreddit,
                    sortType,
                    config.Limit);

                await Task.Delay(2000, ct);

                var response = await _httpClient.GetAsync(url, ct);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throw new InvalidOperationException(
                        $"Превышен лимит запросов Reddit для r/{config.Subreddit}. Повторите позже.");
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException($"Сабреддит r/{config.Subreddit} не найден на Reddit.");
                }

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync(ct);
                var redditResponse = JsonSerializer.Deserialize<RedditResponse>(jsonString, JsonOptions);

                if (redditResponse?.Data?.Children == null || redditResponse.Data.Children.Count == 0)
                {
                    _logger.LogWarning(
                        "Не удалось получить посты из Reddit для r/{Subreddit}",
                        config.Subreddit);
                    return new List<NewsItem>();
                }

                var newsItems = redditResponse.Data.Children
                    .Where(c => c.Data != null)
                    .Select(c => c.Data!)
                    .Take(config.Limit)
                    .Select(ConvertPostToNewsItem)
                    .ToList();

                _logger.LogInformation(
                    "Получено {Count} постов из r/{Subreddit}",
                    newsItems.Count,
                    config.Subreddit);

                return newsItems;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка сети при загрузке Reddit r/{Subreddit}", config.Subreddit);
                throw new InvalidOperationException($"Не удалось загрузить посты из r/{config.Subreddit}.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Ошибка парсинга JSON от Reddit r/{Subreddit}", config.Subreddit);
                throw new InvalidOperationException("Не удалось обработать ответ Reddit.", ex);
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Ошибка при загрузке Reddit r/{Subreddit}", config.Subreddit);
                throw new InvalidOperationException($"Не удалось загрузить посты из r/{config.Subreddit}.", ex);
            }
        }

        public string ValidateAndNormalize(JsonElement json) =>
            RedditSourceConfiguration.ValidateAndNormalizeFromJson(json);

        public IEnumerable<string> GetSuggestedTags(string json)
        {
            var config = JsonSerializer.Deserialize<RedditSourceConfiguration>(json, JsonOptions);
            return config?.Category != null ? new List<string> { config.Category } : new List<string>();
        }

        private static RedditSourceConfiguration DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<RedditSourceConfiguration>(json, SourceConfigurationJson.Options)
                ?? throw new ArgumentException("Некорректная конфигурация Reddit.");

            if (config.Limit == 0)
            {
                config.Limit = defaultLimit;
            }

            return config;
        }

        private static NewsItem ConvertPostToNewsItem(RedditPost post)
        {
            var title = post.Title ?? "Untitled";
            var description = post.SelfText ?? string.Empty;

            var link = !string.IsNullOrEmpty(post.Url) && post.Url.StartsWith("http")
                ? post.Url
                : $"https://www.reddit.com{post.Permalink}";

            string? imageUrl = null;

            if (post.Preview?.Images != null && post.Preview.Images.Any())
            {
                var rawUrl = post.Preview.Images.First().Source?.Url;
                imageUrl = rawUrl != null
                    ? WebUtility.HtmlDecode(rawUrl)
                    : null;
            }
            else if (!string.IsNullOrEmpty(post.Thumbnail) &&
                     post.Thumbnail.StartsWith("http"))
            {
                imageUrl = WebUtility.HtmlDecode(post.Thumbnail);
            }

            return new NewsItem
            {
                Title = title,
                Description = description,
                Link = link,
                PublishedAtUtc = DateTimeOffset.FromUnixTimeSeconds((long)post.CreatedUtc).UtcDateTime,
                SourceItemId = post.Id ?? Guid.NewGuid().ToString(),
                Author = post.Author,
                ImageUrl = imageUrl,
                Category = "Reddit",
                IndexedAtUtc = DateTime.UtcNow
            };
        }

        private class RedditResponse
        {
            [JsonPropertyName("data")]
            public RedditData? Data { get; set; }
        }

        private class RedditData
        {
            [JsonPropertyName("children")]
            public List<RedditChild> Children { get; set; } = new();
        }

        private class RedditChild
        {
            [JsonPropertyName("data")]
            public RedditPost? Data { get; set; }
        }

        private class RedditPost
        {
            [JsonPropertyName("id")]
            public string? Id { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("selftext")]
            public string? SelfText { get; set; }

            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("permalink")]
            public string? Permalink { get; set; }

            [JsonPropertyName("author")]
            public string? Author { get; set; }

            [JsonPropertyName("created_utc")]
            public double CreatedUtc { get; set; }

            [JsonPropertyName("thumbnail")]
            public string? Thumbnail { get; set; }

            [JsonPropertyName("preview")]
            public RedditPreview? Preview { get; set; }
        }

        private class RedditPreview
        {
            [JsonPropertyName("images")]
            public List<RedditImage>? Images { get; set; }
        }

        private class RedditImage
        {
            [JsonPropertyName("source")]
            public RedditImageSource? Source { get; set; }
        }

        private class RedditImageSource
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }
        }
    }
}
