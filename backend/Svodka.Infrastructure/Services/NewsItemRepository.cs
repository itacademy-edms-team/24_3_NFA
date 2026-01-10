using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Data;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Репозиторий для работы с новостями
    /// </summary>
    public class NewsItemRepository : INewsItemRepository
    {
        private readonly NewsAggregatorDbContext _context;

        /// <summary>
        /// Конструктор репозитория новостей
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public NewsItemRepository(NewsAggregatorDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Сохраняет коллекцию новостей в базу данных
        /// </summary>
        /// <param name="newsItems">Коллекция новостей для сохранения</param>
        /// <returns>Задача выполнения операции</returns>
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
        /// <returns>Коллекция новостей</returns>
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
                .Include(n => n.NewsSource)
                .OrderByDescending(n => n.PublishedAtUtc)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }
    }
}