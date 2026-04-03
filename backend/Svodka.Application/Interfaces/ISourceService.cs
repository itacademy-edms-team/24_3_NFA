using Svodka.Application.DTOs;
using Svodka.Domain.Entities;

namespace Svodka.Application.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для управления источниками новостей
    /// </summary>
    public interface ISourceService
    {
        /// <summary>
        /// Получает все источники новостей
        /// </summary>
        /// <returns>Список источников</returns>
        Task<IEnumerable<NewsSource>> GetAllSourcesAsync();

        /// <summary>
        /// Получает источник по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <returns>Источник или null</returns>
        Task<NewsSource?> GetSourceByIdAsync(int id);

        /// <summary>
        /// Создает новый источник
        /// </summary>
        /// <param name="dto">Данные для создания</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Созданный источник</returns>
        Task<NewsSource> CreateSourceAsync(SourceDto dto, CancellationToken ct);

        /// <summary>
        /// Обновляет существующий источник
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="dto">Обновленные данные</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Обновленный источник или null</returns>
        Task<NewsSource?> UpdateSourceAsync(int id, SourceDto dto, CancellationToken ct);

        /// <summary>
        /// Удаляет источник
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <returns>Результат удаления</returns>
        Task<bool> DeleteSourceAsync(int id);

        /// <summary>
        /// Получает опции фильтрации
        /// </summary>
        /// <returns>Объект с опциями фильтрации</returns>
        Task<object> GetFilterOptionsAsync();
    }
}
