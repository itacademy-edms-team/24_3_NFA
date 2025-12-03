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
        /// <param name="cancellationToken">Токен отмены операции.</param>
        Task ExecuteAsync(int? sourceId = null, CancellationToken cancellationToken = default);
    }
}

