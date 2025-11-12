using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svoka.Infrastructure.Data;

namespace Svoka.Infrastructure.Services
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
    }
}