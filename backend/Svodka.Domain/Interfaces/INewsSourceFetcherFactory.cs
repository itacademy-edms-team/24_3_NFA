using Svodka.Domain.Enums;

namespace Svodka.Domain.Interfaces
{
    /// <summary>
    /// Возвращает fetcher новостей по типу источника.
    /// </summary>
    public interface INewsSourceFetcherFactory
    {
        INewsSourceFetcher GetFetcher(SourceType sourceType);
    }
}
