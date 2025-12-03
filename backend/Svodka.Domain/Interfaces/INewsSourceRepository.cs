using Svodka.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для управления источниками новостей
    /// </summary>
    public interface INewsSourceRepository
    {
        /// <summary>
        /// Добавляет новый источник новостей в базу данных
        /// </summary>
        /// <param name="source">Источник новостей для добавления</param>
        /// <returns>Задача выполнения операции</returns>
        Task AddNewsSourceAsync(NewsSource source);

        /// <summary>
        /// Удаляет источник новостей по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор источника для удаления</param>
        /// <returns>Флаг успешности удаления</returns>
        Task<bool> DeleteNewsSourceAsync(int id);

        /// <summary>
        /// Получает все активные источники новостей
        /// </summary>
        /// <returns>Коллекция активных источников</returns>
        Task<IEnumerable<NewsSource>> GetActiveNewsSourcesAsync();

        /// <summary>
        /// Получает источник новостей по его идентификатору
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <returns>Источник новостей или null, если не найден</returns>
        Task<NewsSource?> GetByIdAsync(int id);

        /// <summary>
        /// Получает все источники новостей
        /// </summary>
        /// <returns>Коллекция всех источников</returns>
        Task<IEnumerable<NewsSource>> GetAllSourcesAsync();

        /// <summary>
        /// Обновляет время последнего опроса для источника
        /// </summary>
        /// <param name="sourseId">Идентификатор источника</param>
        /// <param name="utcNow">Время последнего опроса в формате UTC</param>
        /// <returns>Задача выполнения операции</returns>
        Task UpdateLastPolledAtAsync(int sourseId, System.DateTime utcNow);

        /// <summary>
        /// Обновляет информацию об ошибке для источника
        /// </summary>
        /// <param name="sourseId">Идентификатор источника</param>
        /// <param name="utcNow">Время возникновения ошибки в формате UTC</param>
        /// <param name="error">Текст ошибки</param>
        /// <returns>Задача выполнения операции</returns>
        Task UpdateLastErrorAsync(int sourseId, System.DateTime utcNow, string error );

        /// <summary>
        /// Сохраняет все изменения, отслеживаемые контекстом, в базу данных
        /// </summary>
        /// <returns>Задача выполнения операции</returns>
        Task SaveChangesAsync();
    }
}
