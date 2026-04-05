using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using System.Text.Json;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс для провайдеров (поставщиков) новостей из различных внешних источников
    /// </summary>
    public interface INewsProvider
    {
        /// <summary>
        /// Тип источника, который поддерживает данный провайдер
        /// </summary>
        SourceType Type { get; }

        /// <summary>
        /// Получает список новостей из внешнего источника на основе конфигурации
        /// </summary>
        /// <param name="configuration">Объект конфигурации для конкретного типа источника</param>
        /// <returns>Коллекция элементов новостей</returns>
        Task<IEnumerable<NewsItem>> GetNewsAsync(object configuration);

        /// <summary>
        /// Проверяет и нормализует JSON-конфигурацию для данного типа источника
        /// </summary>
        /// <param name="json">Исходный JSON элемент</param>
        /// <returns>Нормализованная JSON-строка</returns>
        /// <exception cref="ArgumentException">Выбрасывается, если конфигурация невалидна</exception>
        string ValidateAndNormalize(JsonElement json);

        /// <summary>
        /// Десериализует JSON-конфигурацию в объект и применяет лимиты по умолчанию
        /// </summary>
        /// <param name="json">JSON-строка конфигурации</param>
        /// <param name="defaultLimit">Лимит по умолчанию</param>
        /// <returns>Объект конфигурации</returns>
        object DeserializeConfiguration(string json, int defaultLimit);

        /// <summary>
        /// Извлекает категорию из JSON-конфигурации
        /// </summary>
        /// <param name="json">JSON-строка конфигурации</param>
        /// <returns>Название категории или null</returns>
        string? GetCategory(string json);
    }
}
