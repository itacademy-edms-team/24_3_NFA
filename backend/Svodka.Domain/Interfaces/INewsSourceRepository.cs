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
        /// Добавляет новый источник новостей.
        /// </summary>
        Task AddNewsSourceAsync(NewsSource source);

        /// <summary>
        /// Получает все активные источники.
        /// </summary>
        Task<IEnumerable<NewsSource>> GetActiveNewsSourcesAsync();

        /// <summary>
        /// Получает источник новостей по его идентификатору.
        /// </summary>
        Task<NewsSource?> GetByIdAsync(int id);

        /// <summary>
        /// Получает все источники новостей.
        /// </summary>
        Task<IEnumerable<NewsSource>> GetAllSourcesAsync();

        /// <summary>
        /// Обновляет время последнего опроса для источника.
        /// </summary>
        Task UpdateLastPolledAtAsync(int sourseId, System.DateTime utcNow);

        /// <summary>
        /// Обновляет информацию об ошибке для источника.
        /// </summary>
        Task UpdateLastErrorAsync(int sourseId, System.DateTime utcNow, string error );

        /// <summary>
        /// Сохраняет все изменения, отслеживаемые контекстом, в базу данных.
        /// </summary>
        Task SaveChangesAsync();
    }
}
