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
        /// Получает все источники новостей пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Список источников</returns>
        Task<IEnumerable<NewsSource>> GetAllSourcesByUserIdAsync(int userId);

        /// <summary>
        /// Получает источник по идентификатору и пользователю
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Источник или null</returns>
        Task<NewsSource?> GetSourceByIdAndUserIdAsync(int id, int userId);

        /// <summary>
        /// Создает новый источник для пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="dto">Данные для создания</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Созданный источник</returns>
        Task<NewsSource> CreateSourceAsync(int userId, SourceDto dto, CancellationToken ct);

        /// <summary>
        /// Обновляет существующий источник
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="dto">Обновленные данные</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Обновленный источник или null</returns>
        Task<NewsSource?> UpdateSourceAsync(int id, int userId, SourceDto dto, CancellationToken ct);

        /// <summary>
        /// Удаляет источник
        /// </summary>
        /// <param name="id">Идентификатор источника</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Результат удаления</returns>
        Task<bool> DeleteSourceAsync(int id, int userId);

        /// <summary>
        /// Получает опции фильтрации для пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Объект с опциями фильтрации</returns>
        Task<object> GetFilterOptionsAsync(int userId);
    }
}
