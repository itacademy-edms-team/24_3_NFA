using Microsoft.EntityFrameworkCore;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Data;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Репозиторий для работы с источниками новостей
    /// </summary>
    public class NewsSourceRepository : INewsSourceRepository
    {
        private readonly NewsAggregatorDbContext _context;

        /// <summary>
        /// Конструктор репозитория источников новостей
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public NewsSourceRepository(NewsAggregatorDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получает все активные источники новостей
        /// </summary>
        /// <returns>Коллекция активных источников</returns>
        public async Task<IEnumerable<NewsSource>> GetActiveNewsSourcesAsync()
        {
            return await _context.NewsSources
                                  .Where(ns => ns.IsActive)
                                  .ToListAsync();
        }

        /// <summary>
        /// Получает источник новостей по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <returns>Источник новостей или null, если не найден</returns>
        public async Task<NewsSource?> GetByIdAsync(int id)
        {
            return await _context.NewsSources.FindAsync(id);
        }

        /// <summary>
        /// Получает все источники новостей
        /// </summary>
        /// <returns>Коллекция всех источников</returns>
        public async Task<IEnumerable<NewsSource>> GetAllSourcesAsync()
        {
            return await _context.NewsSources.ToListAsync();
        }

        /// <summary>
        /// Удаляет источник новостей по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор источника для удаления</param>
        /// <returns>Флаг успешности удаления</returns>
        public async Task<bool> DeleteNewsSourceAsync(int id)
        {
            var source = await _context.NewsSources.FindAsync(id);
            if (source == null) return false;

            _context.NewsSources.Remove(source);
            return true;
        }

        /// <summary>
        /// Обновляет время последнего опроса для источника
        /// </summary>
        /// <param name="sourceId">Идентификатор источника</param>
        /// <param name="utcNow">Время последнего опроса в формате UTC</param>
        /// <returns>Задача выполнения операции</returns>
        public async Task UpdateLastPolledAtAsync(int sourceId, DateTime utcNow)
        {
            var source = await _context.NewsSources.FindAsync(sourceId);
            if (source != null)
            {
                source.LastPolledAtUtc = utcNow;

            }
        }

        /// <summary>
        /// Обновляет информацию об ошибке для источника
        /// </summary>
        /// <param name="sourceId">Идентификатор источника</param>
        /// <param name="utcNow">Время возникновения ошибки в формате UTC</param>
        /// <param name="error">Текст ошибки</param>
        /// <returns>Задача выполнения операции</returns>
        public async Task UpdateLastErrorAsync(int sourceId, DateTime utcNow, string error)
        {
            var source = await _context.NewsSources.FindAsync(sourceId);
            if (source != null)
            {
                source.LastErrorAtUtc = utcNow;
                source.LastError = error;
            }
        }

        /// <summary>
        /// Добавляет новый источник новостей в базу данных
        /// </summary>
        /// <param name="source">Источник новостей для добавления</param>
        /// <returns>Задача выполнения операции</returns>
        public async Task AddNewsSourceAsync(NewsSource source)
        {
            _context.NewsSources.Add(source);
        }

        /// <summary>
        /// Сохраняет все изменения, отслеживаемые контекстом, в базу данных
        /// </summary>
        /// <returns>Задача выполнения операции</returns>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}