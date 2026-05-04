using System.Net;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;

namespace Svodka.Infrastructure.Fetchers
{
    public class RssNewsSourceFetcher : INewsSourceFetcher
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RssNewsSourceFetcher> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RssNewsSourceFetcher(HttpClient httpClient, ILogger<RssNewsSourceFetcher> logger)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Svodka RSS Aggregator 1.0");
            _logger = logger;
        }

        public SourceType Type => SourceType.Rss;

        public async Task<IEnumerable<NewsItem>> FetchAsync(
            NewsSource source,
            int defaultLimit,
            CancellationToken ct = default)
        {
            var config = DeserializeConfiguration(source.Configuration, defaultLimit);

            try
            {
                var response = await _httpClient.GetAsync(config.Url, ct);
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

                    var publishedDate = item.PublishDate.UtcDateTime;
                    var description = ExtractDescription(item);
                    var imageUrl = ExtractImageUrl(item, description);
                    description = StripHtml(description);

                    items.Add(new NewsItem
                    {
                        Title = item.Title?.Text ?? string.Empty,
                        Description = description,
                        Link = item.Links?.FirstOrDefault()?.Uri?.ToString() ?? string.Empty,
                        PublishedAtUtc = publishedDate,
                        SourceItemId = sourceItemId ?? Guid.NewGuid().ToString(),
                        Author = item.Authors?.FirstOrDefault()?.Name,
                        ImageUrl = imageUrl,
                        Category = null,
                        IndexedAtUtc = DateTime.UtcNow
                    });
                }

                return items;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке RSS-ленты: {Url}", config.Url);
                throw new InvalidOperationException($"Не удалось загрузить RSS-ленту: {config.Url}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при парсинге RSS-ленты: {Url}", config.Url);
                throw new InvalidOperationException("Не удалось обработать RSS-ленту. Проверьте URL и формат ленты.", ex);
            }
        }

        public string ValidateAndNormalize(JsonElement json) =>
            RssSourceConfiguration.ValidateAndNormalizeFromJson(json);

        public IEnumerable<string> GetSuggestedTags(string json)
        {
            var config = JsonSerializer.Deserialize<RssSourceConfiguration>(json, JsonOptions);
            return config?.Category != null ? new List<string> { config.Category } : new List<string>();
        }

        private static RssSourceConfiguration DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<RssSourceConfiguration>(json, SourceConfigurationJson.Options)
                ?? throw new ArgumentException("Некорректная конфигурация RSS.");
            if (config.Limit == 0)
            {
                config.Limit = defaultLimit;
            }
            return config;
        }

        private static string ExtractDescription(SyndicationItem item)
        {
            var description = string.Empty;

            if (item.Content != null)
            {
                if (item.Content is TextSyndicationContent textContent)
                {
                    description = textContent.Text;
                }
                else if (item.Content is XmlSyndicationContent xmlContent)
                {
                    using var reader = xmlContent.GetReaderAtContent();
                    var element = XElement.Load(reader);
                    description = element.Value;
                }
            }

            if (string.IsNullOrWhiteSpace(description) && item.Summary != null)
            {
                description = item.Summary.Text ?? string.Empty;
            }

            return description;
        }

        private static string? ExtractImageUrl(SyndicationItem item, string description)
        {
            if (item.ElementExtensions != null)
            {
                foreach (var ext in item.ElementExtensions)
                {
                    if (ext.OuterName == "thumbnail" ||
                        ext.OuterName == "image" ||
                        ext.OuterName == "enclosure")
                    {
                        var urlAttr = ext.GetObject<XElement>()?.Attribute("url")?.Value;
                        if (!string.IsNullOrEmpty(urlAttr))
                        {
                            return urlAttr;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(description))
            {
                var imgMatch = Regex.Match(
                    description,
                    @"<img[^>]+src=[""']([^""']+)[""']",
                    RegexOptions.IgnoreCase);

                if (imgMatch.Success)
                {
                    return imgMatch.Groups[1].Value;
                }
            }

            return null;
        }

        private static string StripHtml(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return description;
            }

            description = Regex.Replace(description, @"<[^>]+>", string.Empty);
            description = WebUtility.HtmlDecode(description);
            return description.Trim();
        }
    }
}
