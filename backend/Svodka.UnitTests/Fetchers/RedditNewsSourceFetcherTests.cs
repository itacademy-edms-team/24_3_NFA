using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Infrastructure.Fetchers;
using Svodka.UnitTests.Helpers;

namespace Svodka.UnitTests.Fetchers
{
    public class RedditNewsSourceFetcherTests
    {
        [Fact]
        public async Task FetchAsync_WithValidConfiguration_ReturnsNewsItems()
        {
            var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""
                {
                  "data": {
                    "children": [
                      {
                        "data": {
                          "id": "post-1",
                          "title": "Test Post",
                          "selftext": "Body",
                          "url": "https://example.com/post",
                          "permalink": "/r/programming/comments/post-1",
                          "author": "tester",
                          "created_utc": 1713960000
                        }
                      }
                    ]
                  }
                }
                """));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Reddit,
                Configuration = """{"Subreddit":"programming","SortType":"hot","Limit":10}"""
            };

            var result = (await fetcher.FetchAsync(source, 20)).ToList();

            Assert.Single(result);
            Assert.Equal("post-1", result[0].SourceItemId);
            Assert.Equal("Reddit", result[0].Category);
            Assert.Contains("/r/programming/hot.json?limit=10", handler.Requests.Single().RequestUri!.ToString());
        }

        [Fact]
        public async Task FetchAsync_WhenLimitIsZero_UsesDefaultLimit()
        {
            var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""{"data":{"children":[]}}"""));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Reddit,
                Configuration = """{"Subreddit":"programming","SortType":"hot","Limit":0}"""
            };

            await fetcher.FetchAsync(source, 12);

            Assert.Contains("limit=12", handler.Requests.Single().RequestUri!.ToString());
        }

        [Fact]
        public void ValidateAndNormalize_WithInvalidConfiguration_ThrowsArgumentException()
        {
            var fetcher = CreateFetcher(new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("{}")));
            using var document = System.Text.Json.JsonDocument.Parse("""{"sortType":"hot"}""");

            Assert.Throws<ArgumentException>(() => fetcher.ValidateAndNormalize(document.RootElement));
        }

        [Fact]
        public void GetSuggestedTags_WithCamelCaseCategory_ReturnsCategory()
        {
            var fetcher = CreateFetcher(new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("{}")));

            var result = fetcher.GetSuggestedTags("""{"category":"Programming"}""");

            Assert.Equal(new[] { "Programming" }, result);
        }

        [Fact]
        public async Task FetchAsync_WhenRedditReturnsTooManyRequests_ThrowsInvalidOperation()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.TooManyRequests));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Reddit,
                Configuration = """{"Subreddit":"programming","SortType":"hot","Limit":10}"""
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => fetcher.FetchAsync(source, 10));
        }

        private static RedditNewsSourceFetcher CreateFetcher(HttpMessageHandler handler)
        {
            return new RedditNewsSourceFetcher(
                new HttpClient(handler),
                Mock.Of<ILogger<RedditNewsSourceFetcher>>());
        }
    }
}
