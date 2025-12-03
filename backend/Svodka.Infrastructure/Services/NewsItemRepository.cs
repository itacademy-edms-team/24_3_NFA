using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Data;

namespace Svodka.Infrastructure.Services
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
            var newsList = newsItems.ToList();
            if (!newsList.Any())
            {
                return;
            }

            var uniqueKeyStrings = newsList
                .Select(n => $"{n.SourceId}:{n.SourceItemId}")
                .Distinct()
                .ToList();

            var existingKeySet = new HashSet<string>(
                await _context.NewsItems
                              .Where(n => uniqueKeyStrings.Contains(n.SourceId + ":" + n.SourceItemId))
                              .Select(n => n.SourceId + ":" + n.SourceItemId)
                              .ToListAsync());

            var newsToInsert = newsList
                .Where(n => !existingKeySet.Contains($"{n.SourceId}:{n.SourceItemId}"))
                .ToList();

            if (!newsToInsert.Any())
            {
                return;
            }

            _context.NewsItems.AddRange(newsToInsert);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NewsItem>> GetLatestNewsAsync(
            int limit,
            string? searchQuery = null,
            DateTime? fromDateUtc = null,
            List<int>? sourceIds = null,
            List<string>? categories = null,
            int offset = 0,
            string? sourceType = null)
        {
            IQueryable<NewsItem> query = _context.NewsItems;

            // Фильтрация по типу источника
            if (!string.IsNullOrWhiteSpace(sourceType))
            {
                query = query.Where(n => n.NewsSource != null && n.NewsSource.Type.ToLower() == sourceType.ToLower());
            }

            // Фильтрация по поисковому запросу
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(n =>
                    n.Title.Contains(searchQuery) ||
                    n.Description.Contains(searchQuery));
            }

            // Фильтрация по дате публикации
            if (fromDateUtc.HasValue)
            {
                query = query.Where(n => n.PublishedAtUtc >= fromDateUtc.Value);
            }

            // Фильтрация по источникам
            if (sourceIds != null && sourceIds.Any())
            {
                query = query.Where(n => sourceIds.Contains(n.SourceId));
            }

            // Фильтрация по категориям
            if (categories != null && categories.Any())
            {
                query = query.Where(n => categories.Contains(n.Category));
            }

            return await query
                .OrderByDescending(n => n.PublishedAtUtc)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }
    }
}