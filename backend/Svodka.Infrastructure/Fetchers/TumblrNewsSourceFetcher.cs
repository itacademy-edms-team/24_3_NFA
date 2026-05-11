using System.Net;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Services;

namespace Svodka.Infrastructure.Fetchers
{
    /// <summary>
    /// Загрузка постов Tumblr через API v2 (при наличии ConsumerKey) или публичный RSS.
    /// </summary>
    public class TumblrNewsSourceFetcher : INewsSourceFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TumblrNewsSourceFetcher> _logger;
        private readonly TumblrSettings _tumblrSettings;

        public TumblrNewsSourceFetcher(
            HttpClient httpClient,
            ILogger<TumblrNewsSourceFetcher> logger,
            IOptions<TumblrSettings> tumblrOptions)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Svodka Tumblr Aggregator 1.0");
            _logger = logger;
            _tumblrSettings = tumblrOptions.Value;
        }

        public SourceType Type => SourceType.Tumblr;

        public async Task<IEnumerable<NewsItem>> FetchAsync(
            NewsSource source,
            int defaultLimit,
            CancellationToken ct = default)
        {
            var config = DeserializeConfiguration(source.Configuration, defaultLimit);

            if (!string.IsNullOrWhiteSpace(_tumblrSettings.ConsumerKey))
            {
                return await FetchViaApiAsync(config, ct);
            }

            _logger.LogInformation(
                "Tumblr:ConsumerKey не задан, используется RSS для блога {Blog}",
                config.BlogName);
            return await FetchViaRssAsync(config, ct);
        }

        public string ValidateAndNormalize(JsonElement json) =>
            TumblrSourceConfiguration.ValidateAndNormalizeFromJson(json);

        public IEnumerable<string> GetSuggestedTags(string json)
        {
            var config = JsonSerializer.Deserialize<TumblrSourceConfiguration>(json, SourceConfigurationJson.Options);
            var tags = new List<string> { "Tumblr" };
            if (!string.IsNullOrWhiteSpace(config?.Category))
            {
                tags.Add(config.Category);
            }
            return tags;
        }

        private async Task<IEnumerable<NewsItem>> FetchViaApiAsync(
            TumblrSourceConfiguration config,
            CancellationToken ct)
        {
            var blogId = Uri.EscapeDataString(config.BlogIdentifier);
            var url =
                $"https://api.tumblr.com/v2/blog/{blogId}/posts?api_key={Uri.EscapeDataString(_tumblrSettings.ConsumerKey)}&limit={Math.Min(config.Limit, 20)}";

            try
            {
                _logger.LogInformation("Загрузка Tumblr API: {Blog}", config.BlogIdentifier);

                var response = await _httpClient.GetAsync(url, ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException($"Блог Tumblr «{config.BlogName}» не найден.");
                }
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(ct);
                var apiResponse = JsonSerializer.Deserialize<TumblrApiResponse>(json, SourceConfigurationJson.Options);

                var posts = apiResponse?.Response?.Posts;
                if (posts == null || posts.Count == 0)
                {
                    _logger.LogWarning("Tumblr API не вернул постов для {Blog}", config.BlogName);
                    return new List<NewsItem>();
                }

                return posts
                    .Take(config.Limit)
                    .Select(MapApiPost)
                    .ToList();
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Ошибка Tumblr API для {Blog}", config.BlogName);
                throw new InvalidOperationException(
                    $"Не удалось загрузить посты Tumblr для блога «{config.BlogName}».", ex);
            }
        }

        private static NewsItem MapApiPost(TumblrApiPost post)
        {
            var description = post.Summary ?? post.Body ?? string.Empty;
            if (string.IsNullOrWhiteSpace(description) && post.Photos?.Count > 0)
            {
                description = post.Photos[0].Caption ?? string.Empty;
            }

            return new NewsItem
            {
                Title = string.IsNullOrWhiteSpace(post.Title) ? "Пост Tumblr" : post.Title,
                Description = StripHtml(description),
                Link = post.PostUrl ?? string.Empty,
                PublishedAtUtc = DateTimeOffset.FromUnixTimeSeconds(post.Timestamp).UtcDateTime,
                SourceItemId = post.Id?.ToString() ?? Guid.NewGuid().ToString(),
                Author = post.BlogName,
                ImageUrl = post.Photos?.FirstOrDefault()?.OriginalSize?.Url,
                Category = "Tumblr",
                IndexedAtUtc = DateTime.UtcNow
            };
        }

        private async Task<IEnumerable<NewsItem>> FetchViaRssAsync(
            TumblrSourceConfiguration config,
            CancellationToken ct)
        {
            var rssUrl = config.RssUrl;

            try
            {
                _logger.LogInformation("Загрузка RSS Tumblr: {Url}", rssUrl);

                var response = await _httpClient.GetAsync(rssUrl, ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException($"Блог Tumblr «{config.BlogName}» не найден.");
                }
                response.EnsureSuccessStatusCode();

                var contentStream = await response.Content.ReadAsStreamAsync(ct);
                using var xmlReader = XmlReader.Create(contentStream);
                var feed = SyndicationFeed.Load(xmlReader);

                var items = new List<NewsItem>();
                foreach (var item in feed.Items.Take(config.Limit))
                {
                    var sourceItemId = item.Id;
                    if (string.IsNullOrEmpty(sourceItemId) && item.Links?.Any() == true)
                    {
                        sourceItemId = item.Links.First().Uri.ToString();
                    }

                    var description = ExtractDescription(item);
                    items.Add(new NewsItem
                    {
                        Title = item.Title?.Text ?? string.Empty,
                        Description = StripHtml(description),
                        Link = item.Links?.FirstOrDefault()?.Uri?.ToString() ?? string.Empty,
                        PublishedAtUtc = item.PublishDate.UtcDateTime,
                        SourceItemId = sourceItemId ?? Guid.NewGuid().ToString(),
                        Author = config.BlogName,
                        Category = "Tumblr",
                        IndexedAtUtc = DateTime.UtcNow
                    });
                }

                return items;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Ошибка при загрузке Tumblr RSS: {Url}", rssUrl);
                throw new InvalidOperationException(
                    $"Не удалось загрузить ленту Tumblr для блога «{config.BlogName}».", ex);
            }
        }

        private static TumblrSourceConfiguration DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<TumblrSourceConfiguration>(json, SourceConfigurationJson.Options)
                ?? throw new ArgumentException("Некорректная конфигурация Tumblr.");
            if (config.Limit == 0) config.Limit = defaultLimit;
            return config;
        }

        private static string ExtractDescription(SyndicationItem item)
        {
            if (item.Content is TextSyndicationContent textContent)
            {
                return textContent.Text;
            }
            if (item.Content is XmlSyndicationContent xmlContent)
            {
                using var reader = xmlContent.GetReaderAtContent();
                return XElement.Load(reader).Value;
            }
            return item.Summary?.Text ?? string.Empty;
        }

        private static string StripHtml(string description)
        {
            if (string.IsNullOrEmpty(description)) return description;
            description = Regex.Replace(description, @"<[^>]+>", string.Empty);
            return WebUtility.HtmlDecode(description).Trim();
        }

        private class TumblrApiResponse
        {
            [JsonPropertyName("response")]
            public TumblrApiResponseBody? Response { get; set; }
        }

        private class TumblrApiResponseBody
        {
            [JsonPropertyName("posts")]
            public List<TumblrApiPost>? Posts { get; set; }
        }

        private class TumblrApiPost
        {
            [JsonPropertyName("id")]
            public long? Id { get; set; }

            [JsonPropertyName("blog_name")]
            public string? BlogName { get; set; }

            [JsonPropertyName("post_url")]
            public string? PostUrl { get; set; }

            [JsonPropertyName("timestamp")]
            public long Timestamp { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("body")]
            public string? Body { get; set; }

            [JsonPropertyName("summary")]
            public string? Summary { get; set; }

            [JsonPropertyName("photos")]
            public List<TumblrApiPhoto>? Photos { get; set; }
        }

        private class TumblrApiPhoto
        {
            [JsonPropertyName("caption")]
            public string? Caption { get; set; }

            [JsonPropertyName("original_size")]
            public TumblrApiPhotoSize? OriginalSize { get; set; }
        }

        private class TumblrApiPhotoSize
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }
        }
    }
}
