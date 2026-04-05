using System.Text.Json.Serialization;

namespace Svodka.Domain.Enums
{
    /// <summary>
    /// Тип источника новостей
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SourceType
    {
        /// <summary>
        /// RSS-лента
        /// </summary>
        Rss,

        /// <summary>
        /// GitHub репозиторий
        /// </summary>
        GitHub,

        /// <summary>
        /// Reddit сабреддит
        /// </summary>
        Reddit
    }
}
