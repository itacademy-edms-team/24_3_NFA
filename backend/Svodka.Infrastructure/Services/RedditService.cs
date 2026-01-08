using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Сервис для работы с Reddit JSON API
    /// </summary>
    public class RedditService : IRedditService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RedditService> _logger;
        private const string RedditApiBaseUrl = "https://www.reddit.com";

        public RedditService(HttpClient httpClient, ILogger<RedditService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SvodkaApp/1.0 (Student Project)");
            
        }

        public async Task<IEnumerable<NewsItem>> FetchSubredditPostsAsync(string subreddit, string sortType, int limit)
        {
            try
            {
                string timeFilter = sortType.ToLower() == "top" ? "&t=week" : "";  
                // Reddit JSON API: https://www.reddit.com/r/{subreddit}/{sort}.json
                var url = $"{RedditApiBaseUrl}/r/{subreddit}/{sortType}.json?limit={Math.Min(limit, 100)}{timeFilter}";
                _logger.LogInformation(
                    "Загрузка постов из Reddit: r/{Subreddit} (sort: {SortType}, limit: {Limit})", 
                    subreddit, sortType, limit);

                await Task.Delay(2000); 

                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Reddit API вернул 429 (Too Many Requests). Слишком частые запросы к r/{Subreddit}", subreddit);
                    return new List<NewsItem>(); 
                }

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var redditResponse = JsonSerializer.Deserialize<RedditResponse>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (redditResponse?.Data?.Children == null || redditResponse.Data.Children.Count == 0)
                {
                    _logger.LogWarning(
                        "Не удалось получить посты из Reddit для r/{Subreddit}", 
                        subreddit);
                    return new List<NewsItem>();
                }

                var posts = redditResponse.Data.Children
                    .Where(c => c.Data != null)
                    .Select(c => c.Data)
                    .Take(limit)
                    .ToList();

                var newsItems = posts
                    .Select(ConvertPostToNewsItem)
                    .Where(item => item != null)
                    .ToList();

                _logger.LogInformation(
                    "Получено {Count} постов из r/{Subreddit}", 
                    newsItems.Count, 
                    subreddit);

                return newsItems;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex, 
                    "Ошибка сети при загрузке постов из Reddit r/{Subreddit}", 
                    subreddit);
                return new List<NewsItem>(); 
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex, 
                    "Ошибка парсинга JSON от Reddit для r/{Subreddit}", 
                    subreddit);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Непредвиденная ошибка при загрузке постов из Reddit r/{Subreddit}", 
                    subreddit);
                throw;
            }
        }

        private NewsItem ConvertPostToNewsItem(RedditPost post)
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
