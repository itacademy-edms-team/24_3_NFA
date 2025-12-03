using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Net;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Служба для работы с RSS-лентами
    /// </summary>
    public class RssService: IRssService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RssService> _logger;

        /// <summary>
        /// Конструктор службы RSS
        /// </summary>
        /// <param name="httpClient">HTTP клиент для выполнения запросов</param>
        /// <param name="logger">Логгер</param>
        public RssService(HttpClient httpClient, ILogger<RssService> logger)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Svodka RSS Aggregator 1.0");
            _logger = logger;
        }

        /// <summary>
        /// Загружает и парсит RSS-ленту по URL
        /// </summary>
        /// <param name="url">URL RSS-ленты</param>
        /// <param name="limit">Максимальное количество элементов для возврата</param>
        /// <returns>Список новостей из ленты</returns>
        public async  Task<IEnumerable<NewsItem>> FetchRssFeedAsync(string url, int limit)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var contentStream = await response.Content.ReadAsStreamAsync();

                using var xmlReader = XmlReader.Create(contentStream);
                var feed = SyndicationFeed.Load(xmlReader);

                var items = new List<NewsItem>();

                foreach (var item in feed.Items.Take(limit))
                {
                    var sourceItemId = item.Id;

                    if (string.IsNullOrEmpty(sourceItemId) && item.Links?.Any() == true)
                    {
                        sourceItemId = item.Links.First().Uri.ToString();
                    }

                    var publishedDate = item.PublishDate.UtcDateTime;

                    // Извлекаем полный текст из Content или Summary
                    string description = string.Empty;
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
                            description = element.Value; // только текстовое содержимое без тегов
                        }

                    }

                    if (string.IsNullOrWhiteSpace(description) && item.Summary != null)
                    {
                        description = item.Summary.Text ?? string.Empty;
                    }

                    // Извлекаем картинку из различных мест RSS
                    string? imageUrl = null;
                    
                    // Проверяем медиа-контент (Media RSS)
                    if (item.ElementExtensions != null)
                    {
                        foreach (var ext in item.ElementExtensions)
                        {
                            if (ext.OuterName == "thumbnail" || ext.OuterName == "image" || ext.OuterName == "enclosure")
                            {
                                var urlAttr = ext.GetObject<XElement>()?.Attribute("url")?.Value;
                                if (!string.IsNullOrEmpty(urlAttr))
                                {
                                    imageUrl = urlAttr;
                                    break;
                                }
                            }
                        }
                    }

                    // Проверяем в описании на наличие img тегов
                    if (string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(description))
                    {
                        var imgMatch = Regex.Match(
                            description, 
                            @"<img[^>]+src=[""']([^""']+)[""']", 
                            RegexOptions.IgnoreCase);
                        if (imgMatch.Success)
                        {
                            imageUrl = imgMatch.Groups[1].Value;
                        }
                    }

                    // Удаляем HTML теги из описания для чистого текста
                    if (!string.IsNullOrEmpty(description))
                    {
                        description = Regex.Replace(
                            description, 
                            @"<[^>]+>", 
                            string.Empty);
                        description = WebUtility.HtmlDecode(description);
                        description = description.Trim();
                    }

                    var newsItem = new NewsItem
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
                    };

                    items.Add(newsItem);
                }

                return items;

            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке или парсинге RSS-ленты: {Url}", url);
                throw;
            }
        }
    }
}
