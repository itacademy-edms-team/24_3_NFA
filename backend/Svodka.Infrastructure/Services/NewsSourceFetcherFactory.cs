using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;

namespace Svodka.Infrastructure.Services
{
    /// <summary>
    /// Возвращает зарегистрированные fetcher'ы новостей по типу источника.
    /// </summary>
    public class NewsSourceFetcherFactory : INewsSourceFetcherFactory
    {
        private readonly Dictionary<SourceType, INewsSourceFetcher> _fetchers;

        public NewsSourceFetcherFactory(IEnumerable<INewsSourceFetcher> fetchers)
        {
            _fetchers = fetchers.ToDictionary(f => f.Type);
        }

        public INewsSourceFetcher GetFetcher(SourceType sourceType)
        {
            if (_fetchers.TryGetValue(sourceType, out var fetcher))
            {
                return fetcher;
            }

            throw new ArgumentException($"Неизвестный тип fetcher'а новостей: {sourceType}", nameof(sourceType));
        }
    }
}
