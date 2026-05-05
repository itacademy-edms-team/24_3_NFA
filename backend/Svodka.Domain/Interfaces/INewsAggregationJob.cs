using System.Threading;
using System.Threading.Tasks;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Описывает единичный запуск процесса агрегации новостей.
    /// </summary>
    public interface INewsAggregationJob
    {
        /// <summary>
        /// Выполняет агрегацию новостей.
        /// </summary>
        /// <param name="sourceId">
        /// Идентификатор источника. Если не указан, обрабатываются все активные источники.
        /// </param>
        /// <param name="force">
        /// При true — обрабатывать источник даже если IsActive = false (ручной запуск).
        /// </param>
        /// <param name="cancellationToken">Токен отмены операции.</param>
        /// <returns>Количество сохранённых новостей (0 при ошибке или пропуске).</returns>
        Task<int> ExecuteAsync(int? sourceId = null, bool force = false, CancellationToken cancellationToken = default);
    }
}

