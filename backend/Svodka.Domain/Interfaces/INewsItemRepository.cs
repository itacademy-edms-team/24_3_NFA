using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для работы с новостями
    /// </summary>
    public interface INewsItemRepository
    {
        /// <summary>
        /// Сохраняет коллекцию новостей в базу данных
        /// </summary>
        /// <param name="newsItems">Коллекция новостей для сохранения</param>
        /// <returns>Задача выполнения операции</returns>
        Task SaveNewsAsync(IEnumerable<NewsItem> newsItems);

        /// <summary>
        /// Получает последние новости с фильтрацией и пагинацией
        /// </summary>
        /// <param name="limit">Количество новостей для получения</param>
        /// <param name="searchQuery">Поисковый запрос для фильтрации по заголовку или описанию</param>
        /// <param name="fromDateUtc">Дата начала для фильтрации по времени публикации</param>
        /// <param name="sourceIds">Список идентификаторов источников для фильтрации</param>
        /// <param name="categories">Список категорий для фильтрации</param>
        /// <param name="offset">Смещение для пагинации</param>
        /// <param name="sourceType">Тип источника для фильтрации</param>
        /// <param name="orderBy">Поле для сортировки (PublishedAtUtc по умолчанию)</param>
        /// <returns>Коллекция новостей</returns>
        Task<IEnumerable<NewsItem>> GetLatestNewsAsync(
            int limit,
            string? searchQuery = null,
            DateTime? fromDateUtc = null,
            List<int>? sourceIds = null,
            List<string>? categories = null,
            int offset = 0,
            string? sourceType = null);
    }
}
