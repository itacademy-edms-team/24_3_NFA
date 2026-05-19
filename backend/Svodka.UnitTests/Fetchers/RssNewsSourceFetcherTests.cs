using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Infrastructure.Fetchers;
using Svodka.UnitTests.Helpers;

namespace Svodka.UnitTests.Fetchers
{
    public class RssNewsSourceFetcherTests
    {
        [Fact]
        public async Task FetchAsync_WithValidConfiguration_ReturnsNewsItems()
        {
            var handler = new FakeHttpMessageHandler(_ => RssResponse("""
                <?xml version="1.0" encoding="utf-8"?>
                <rss version="2.0">
                  <channel>
                    <title>Example</title>
                    <item>
                      <guid>item-1</guid>
                      <title>Test News</title>
                      <link>https://example.com/news</link>
                      <description><![CDATA[<p>Hello</p>]]></description>
                      <pubDate>Wed, 24 Apr 2024 12:00:00 GMT</pubDate>
                      <author>tester@example.com</author>
                    </item>
                  </channel>
                </rss>
                """));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Rss,
                Configuration = """{"url":"https://example.com/rss","limit":10}"""
            };

            var result = (await fetcher.FetchAsync(source, 20)).ToList();

            Assert.Single(result);
            Assert.Equal("item-1", result[0].SourceItemId);
            Assert.Equal("Test News", result[0].Title);
            Assert.Equal("Hello", result[0].Description);
            Assert.Equal("https://example.com/rss", handler.Requests.Single().RequestUri!.ToString());
        }

        [Fact]
        public async Task FetchAsync_WhenLimitIsZero_UsesDefaultLimit()
        {
            var handler = new FakeHttpMessageHandler(_ => RssResponse("""
                <?xml version="1.0" encoding="utf-8"?>
                <rss version="2.0">
                  <channel>
                    <title>Example</title>
                    <item><guid>1</guid><title>One</title><link>https://example.com/1</link></item>
                    <item><guid>2</guid><title>Two</title><link>https://example.com/2</link></item>
                  </channel>
                </rss>
                """));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Rss,
                Configuration = """{"url":"https://example.com/rss","limit":0}"""
            };

            var result = await fetcher.FetchAsync(source, 1);

            Assert.Single(result);
        }

        [Fact]
        public void ValidateAndNormalize_AddsHttps_WhenSchemeIsMissing()
        {
            var fetcher = CreateFetcher(new FakeHttpMessageHandler(_ => RssResponse("<rss version=\"2.0\" />")));
            using var document = System.Text.Json.JsonDocument.Parse("""{"url":"example.com/rss","limit":10}""");

            var normalized = fetcher.ValidateAndNormalize(document.RootElement);

            Assert.Contains("https://example.com/rss", normalized);
        }

        [Fact]
        public void GetSuggestedTags_WithCamelCaseCategory_ReturnsCategory()
        {
            var fetcher = CreateFetcher(new FakeHttpMessageHandler(_ => RssResponse("<rss version=\"2.0\" />")));

            var result = fetcher.GetSuggestedTags("""{"category":"World"}""");

            Assert.Equal(new[] { "World" }, result);
        }

        [Fact]
        public async Task FetchAsync_WhenRssReturnsError_PropagatesException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Rss,
                Configuration = """{"url":"https://example.com/rss","limit":10}"""
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => fetcher.FetchAsync(source, 10));
        }

        private static RssNewsSourceFetcher CreateFetcher(HttpMessageHandler handler)
        {
            return new RssNewsSourceFetcher(
                new HttpClient(handler),
                Mock.Of<ILogger<RssNewsSourceFetcher>>());
        }

        private static HttpResponseMessage RssResponse(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };
        }
    }
}
