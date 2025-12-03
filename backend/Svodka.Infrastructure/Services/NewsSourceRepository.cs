using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Data;

namespace Svodka.Infrastructure.Services
{
    public class NewsSourceRepository : INewsSourceRepository
    {
        private readonly NewsAggregatorDbContext _context;

        public NewsSourceRepository(NewsAggregatorDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<NewsSource>> GetActiveNewsSourcesAsync()
        {
            return await _context.NewsSources
                                  .Where(ns => ns.IsActive)
                                  .ToListAsync();
        }

        public async Task<NewsSource?> GetByIdAsync(int id)
        {
            return await _context.NewsSources.FindAsync(id);
        }

        public async Task<IEnumerable<NewsSource>> GetAllSourcesAsync()
        {
            return await _context.NewsSources.ToListAsync();
        }

        public async Task<bool> DeleteNewsSourceAsync(int id)
        {
            var source = await _context.NewsSources.FindAsync(id);
            if (source == null) return false;

            _context.NewsSources.Remove(source);
            return true;
        }

        public async Task UpdateLastPolledAtAsync(int sourceId, DateTime utcNow)
        {
            var source = await _context.NewsSources.FindAsync(sourceId);
            if (source != null)
            {
                source.LastPolledAtUtc = utcNow;

            }
        }

        public async Task UpdateLastErrorAsync(int sourceId, DateTime utcNow, string error)
        {
            var source = await _context.NewsSources.FindAsync(sourceId);
            if (source != null)
            {
                source.LastErrorAtUtc = utcNow;
                source.LastError = error;
            }
        }

        public async Task AddNewsSourceAsync(NewsSource source)
        {
            _context.NewsSources.Add(source);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}