using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    public interface INewsItemRepository
    {
        Task SaveNewsAsync(IEnumerable<NewsItem> newsItems);

        Task<IEnumerable<NewsItem>> GetLatestNewsAsync(
            int limit,
            string? searchQuery = null,
            DateTime? fromDateUtc = null);
    }
}
