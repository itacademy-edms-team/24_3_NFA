using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс для получения новостей из внешнего источника
    /// </summary>
    public interface INewsProvider
    {
        /// <summary>
        /// Получает список новостей из источника.
        /// </summary>
        /// <param name="configuration">Типизированная конфигурация для источника.</param>
        /// <returns>Список новостей.</returns>
        Task<IEnumerable<NewsItem>> GetNewsAsync(object configuration);

    }
}
