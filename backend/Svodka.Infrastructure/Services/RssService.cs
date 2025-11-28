using Microsoft.Extensions.Logging;
using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Svodka.Infrastructure.Services
{
    public class RssService: IRssService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RssService> _logger;

        public RssService(HttpClient httpClient, ILogger<RssService> logger)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Svodka RSS Aggregator 1.0");
            _logger = logger;
        }

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

                    string? imageUrl = null;
                    var newsItem = new NewsItem
                    {
                        Title = item.Title?.Text ?? string.Empty, 
                        Description = item.Summary?.Text ?? string.Empty, 
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
