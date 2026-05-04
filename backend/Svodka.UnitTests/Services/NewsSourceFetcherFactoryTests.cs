using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Services;

namespace Svodka.UnitTests.Services
{
    public class NewsSourceFetcherFactoryTests
    {
        [Fact]
        public void GetFetcher_ReturnsFetcherForSourceType()
        {
            var rssFetcher = new StubFetcher(SourceType.Rss);
            var factory = new NewsSourceFetcherFactory(new INewsSourceFetcher[]
            {
                rssFetcher,
                new StubFetcher(SourceType.Reddit)
            });

            var result = factory.GetFetcher(SourceType.Rss);

            Assert.Same(rssFetcher, result);
        }

        [Fact]
        public void GetFetcher_WithUnknownSourceType_ThrowsArgumentException()
        {
            var factory = new NewsSourceFetcherFactory(Array.Empty<INewsSourceFetcher>());

            Assert.Throws<ArgumentException>(() => factory.GetFetcher(SourceType.Rss));
        }

        private class StubFetcher : INewsSourceFetcher
        {
            public StubFetcher(SourceType type)
            {
                Type = type;
            }

            public SourceType Type { get; }

            public Task<IEnumerable<NewsItem>> FetchAsync(
                NewsSource source,
                int defaultLimit,
                CancellationToken ct = default)
            {
                return Task.FromResult(Enumerable.Empty<NewsItem>());
            }

            public string ValidateAndNormalize(System.Text.Json.JsonElement json)
            {
                return json.GetRawText();
            }

            public IEnumerable<string> GetSuggestedTags(string json)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
