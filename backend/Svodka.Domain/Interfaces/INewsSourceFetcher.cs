using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using System.Text.Json;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Загружает новости из одного настроенного типа внешнего источника.
    /// </summary>
    public interface INewsSourceFetcher
    {
        SourceType Type { get; }

        Task<IEnumerable<NewsItem>> FetchAsync(
            NewsSource source,
            int defaultLimit,
            CancellationToken ct = default);

        string ValidateAndNormalize(JsonElement json);

        IEnumerable<string> GetSuggestedTags(string json);
    }
}
