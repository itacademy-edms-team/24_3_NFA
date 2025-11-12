using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для управления новостями.
    /// </summary>
    public interface INewsItemRepository
    {
        /// <summary>
        /// Сохраняет коллекцию новостей, автоматически проверя на дубликаты по SourceId и SourceItemId
        /// </summary>
        ///<param name="newsItems">Коллекция новостей для сохранения</param>
        Task SaveNewsAsync(IEnumerable<NewsItem> newsItems);

        Task<IEnumerable<NewsItem>> GetLatestNewsAsync(int limit);
    }
}
