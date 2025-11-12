using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svoka.Infrastructure.Data;

namespace Svoka.Infrastructure.Services
{
    public class NewsItemRepository : INewsItemRepository
    {
        private readonly NewsAggregatorDbContext _context;

        public NewsItemRepository(NewsAggregatorDbContext context)
        {
            _context = context;
        }

        public async Task SaveNewsAsync(IEnumerable<NewsItem> newsItems)
        {
            var uniqueKeys = newsItems.Select(n => new { n.SourceId, n.SourceItemId }).ToList();

            var existingKeys = await _context.NewsItems
                                              .Where(n => uniqueKeys.Contains(new { n.SourceId, n.SourceItemId }))
                                              .Select(n => new { n.SourceId, n.SourceItemId })
                                              .ToListAsync();

            var newsToInsert = newsItems.Where(n => !existingKeys.Contains(new { n.SourceId, n.SourceItemId }));

            _context.NewsItems.AddRange(newsToInsert);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NewsItem>> GetLatestNewsAsync(int limit)
        {
            return await _context.NewsItems
                                  .OrderByDescending(n => n.PublishedAtUtc)
                                  .Take(limit)
                                  .ToListAsync();
        }
    }
}