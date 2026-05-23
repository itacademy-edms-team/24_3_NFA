using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Services;

namespace Svodka.Infrastructure.Fetchers
{
    public class VkNewsSourceFetcher : INewsSourceFetcher
    {
        private static readonly HashSet<string> AllowedFilters = new(StringComparer.OrdinalIgnoreCase)
        {
            "all", "owner", "others", "postponed", "suggests", "donut"
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<VkNewsSourceFetcher> _logger;
        private readonly VkSettings _vkSettings;

        public VkNewsSourceFetcher(
            HttpClient httpClient,
            ILogger<VkNewsSourceFetcher> logger,
            IOptions<VkSettings> vkOptions)
        {
            _httpClient = httpClient;
            _logger = logger;
            _vkSettings = vkOptions.Value;
        }

        public SourceType Type => SourceType.Vk;

        public async Task<IEnumerable<NewsItem>> FetchAsync(
            NewsSource source,
            int defaultLimit,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_vkSettings.ServiceAccessToken))
            {
                throw new InvalidOperationException(
                    "Не настроен VK: задайте Vk:ServiceAccessToken в user-secrets (сервисный ключ мини-приложения).");
            }

            var config = DeserializeConfiguration(source.Configuration, defaultLimit);

            try
            {
                var url = BuildWallGetUrl(config);
                _logger.LogInformation(
                    "Загрузка стены VK: {Target}, filter={Filter}",
                    !string.IsNullOrWhiteSpace(config.Domain) ? $"domain={config.Domain}" : $"owner_id={config.OwnerId}",
                    config.Filter);

                var response = await _httpClient.GetAsync(url, ct);
                var json = await response.Content.ReadAsStringAsync(ct);

                var vkError = TryParseVkError(json);
                if (vkError != null)
                {
                    throw new InvalidOperationException(vkError);
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Ошибка HTTP при запросе к VK: {(int)response.StatusCode}");
                }

                var wall = JsonSerializer.Deserialize<VkWallResponse>(json, SourceConfigurationJson.Options);
                var posts = wall?.Response?.Items;
                if (posts == null || posts.Count == 0)
                {
                    _logger.LogWarning("VK не вернул записей для источника");
                    return new List<NewsItem>();
                }

                var fallbackOwnerId = config.OwnerId ?? posts.FirstOrDefault()?.OwnerId ?? 0;

                return posts
                    .Take(config.Limit)
                    .Select(p => MapPost(p, fallbackOwnerId))
                    .ToList();
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Ошибка при загрузке VK");
                throw new InvalidOperationException("Не удалось загрузить записи VK.", ex);
            }
        }

        public string ValidateAndNormalize(JsonElement json) =>
            VkSourceConfiguration.ValidateAndNormalizeFromJson(json);

        public IEnumerable<string> GetSuggestedTags(string json)
        {
            var config = JsonSerializer.Deserialize<VkSourceConfiguration>(json, SourceConfigurationJson.Options);
            var tags = new List<string> { "VK" };
            if (!string.IsNullOrWhiteSpace(config?.Domain))
            {
                tags.Add(config.Domain);
            }
            if (!string.IsNullOrWhiteSpace(config?.Category))
            {
                tags.Add(config.Category);
            }
            return tags;
        }

        /// <summary>
        /// https://dev.vk.com/method/wall.get — domain или owner_id, offset, count, filter, access_token, v
        /// </summary>
        private string BuildWallGetUrl(VkSourceConfiguration config)
        {
            var count = Math.Min(config.Limit, 100);
            var filter = AllowedFilters.Contains(config.Filter) ? config.Filter : "all";

            var query = new Dictionary<string, string>
            {
                ["count"] = count.ToString(),
                ["offset"] = "0",
                ["filter"] = filter,
                ["access_token"] = _vkSettings.ServiceAccessToken,
                ["v"] = _vkSettings.ApiVersion
            };

            if (!string.IsNullOrWhiteSpace(config.Domain))
            {
                query["domain"] = config.Domain;
            }
            else if (config.OwnerId.HasValue)
            {
                query["owner_id"] = config.OwnerId.Value.ToString();
            }

            var queryString = string.Join(
                "&",
                query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

            return $"https://api.vk.com/method/wall.get?{queryString}";
        }

        private static NewsItem MapPost(VkWallPost post, long fallbackOwnerId)
        {
            var ownerId = post.OwnerId != 0 ? post.OwnerId : fallbackOwnerId;
            var text = post.Text ?? string.Empty;
            var title = text.Length > 120 ? text[..120] + "…" : (string.IsNullOrWhiteSpace(text) ? "Запись VK" : text);
            var published = post.Date > 0
                ? DateTimeOffset.FromUnixTimeSeconds(post.Date).UtcDateTime
                : DateTime.UtcNow;

            return new NewsItem
            {
                Title = title,
                Description = NormalizeText(text),
                Link = $"https://vk.com/wall{ownerId}_{post.Id}",
                PublishedAtUtc = published,
                SourceItemId = $"{ownerId}_{post.Id}",
                Author = ownerId < 0 ? "VK сообщество" : "VK пользователь",
                ImageUrl = post.Attachments?.FirstOrDefault()?.Photo?.Sizes?
                    .OrderByDescending(s => s.Width)
                    .FirstOrDefault()?.Url,
                Category = "VK",
                IndexedAtUtc = DateTime.UtcNow
            };
        }

        private static string NormalizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        private static string? TryParseVkError(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error", out var err))
                {
                    var code = err.TryGetProperty("error_code", out var c) ? c.GetInt32() : 0;
                    var msg = err.TryGetProperty("error_msg", out var m) ? m.GetString() : "ошибка VK API";
                    return $"VK API ({code}): {msg}";
                }
            }
            catch
            {
                // ignore parse errors
            }
            return null;
        }

        private static VkSourceConfiguration DeserializeConfiguration(string json, int defaultLimit)
        {
            var config = JsonSerializer.Deserialize<VkSourceConfiguration>(json, SourceConfigurationJson.Options)
                ?? throw new ArgumentException("Некорректная конфигурация VK.");
            if (config.Limit == 0) config.Limit = defaultLimit;
            return config;
        }

        private class VkWallResponse
        {
            [JsonPropertyName("response")]
            public VkWallResponseBody? Response { get; set; }
        }

        private class VkWallResponseBody
        {
            [JsonPropertyName("items")]
            public List<VkWallPost>? Items { get; set; }
        }

        private class VkWallPost
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("owner_id")]
            public long OwnerId { get; set; }

            [JsonPropertyName("date")]
            public long Date { get; set; }

            [JsonPropertyName("text")]
            public string? Text { get; set; }

            [JsonPropertyName("attachments")]
            public List<VkAttachment>? Attachments { get; set; }
        }

        private class VkAttachment
        {
            [JsonPropertyName("photo")]
            public VkPhoto? Photo { get; set; }
        }

        private class VkPhoto
        {
            [JsonPropertyName("sizes")]
            public List<VkPhotoSize>? Sizes { get; set; }
        }

        private class VkPhotoSize
        {
            [JsonPropertyName("url")]
            public string? Url { get; set; }

            [JsonPropertyName("width")]
            public int Width { get; set; }
        }
    }
}
