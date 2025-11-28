using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Infrastructure.Services
{
    public interface IRssService
    {
        /// <summary>
        /// Загружает и парсит RSS-ленту по URL.
        /// </summary>
        /// <param name="url">URL RSS-ленты.</param>
        /// <param name="limit">Максимальное количество элементов для возврата.</param>
        /// <returns>Список новостей из ленты.</returns>
        Task<IEnumerable<NewsItem>> FetchRssFeedAsync(string url, int limit);
    }
}
