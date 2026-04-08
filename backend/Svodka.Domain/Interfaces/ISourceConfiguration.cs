namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для конфигурации источника новостей
    /// </summary>
    public interface ISourceConfiguration
    {
        /// <summary>
        /// Лимит количества новостей
        /// </summary>
        int Limit { get; set; }

        /// <summary>
        /// Категория источника
        /// </summary>
        string? Category { get; set; }
    }
}
