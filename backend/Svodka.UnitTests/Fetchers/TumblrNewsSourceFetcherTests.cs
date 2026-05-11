using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Infrastructure.Fetchers;
using Svodka.Infrastructure.Services;
using Svodka.UnitTests.Helpers;

namespace Svodka.UnitTests.Fetchers
{
    public class TumblrNewsSourceFetcherTests
    {
        [Fact]
        public async Task FetchAsync_WithApiKey_UsesTumblrApi()
        {
            var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""
                {
                  "response": {
                    "posts": [
                      {
                        "id": 999,
                        "blog_name": "staff",
                        "post_url": "https://staff.tumblr.com/post/999",
                        "timestamp": 1713945600,
                        "title": "Hello Tumblr",
                        "summary": "Post body"
                      }
                    ]
                  }
                }
                """));
            var fetcher = CreateFetcher(handler, "test-consumer-key");
            var source = new NewsSource
            {
                Type = SourceType.Tumblr,
                Configuration = """{"blogName":"staff","limit":10}"""
            };

            var result = (await fetcher.FetchAsync(source, 20)).ToList();

            Assert.Single(result);
            Assert.Equal("999", result[0].SourceItemId);
            Assert.Equal("Hello Tumblr", result[0].Title);
            var requestUri = handler.Requests.Single().RequestUri!.ToString();
            Assert.Contains("api.tumblr.com", requestUri);
            Assert.Contains("api_key=test-consumer-key", requestUri);
        }

        [Fact]
        public async Task FetchAsync_WithoutApiKey_UsesRss()
        {
            var handler = new FakeHttpMessageHandler(_ => RssResponse("""
                <?xml version="1.0" encoding="utf-8"?>
                <rss version="2.0">
                  <channel>
                    <title>staff</title>
                    <item>
                      <guid>rss-1</guid>
                      <title>RSS Post</title>
                      <link>https://staff.tumblr.com/post/1</link>
                      <description>Content</description>
                      <pubDate>Wed, 24 Apr 2024 12:00:00 GMT</pubDate>
                    </item>
                  </channel>
                </rss>
                """));
            var fetcher = CreateFetcher(handler, consumerKey: "");
            var source = new NewsSource
            {
                Type = SourceType.Tumblr,
                Configuration = """{"blogName":"staff","limit":10}"""
            };

            var result = (await fetcher.FetchAsync(source, 20)).ToList();

            Assert.Single(result);
            Assert.Equal("RSS Post", result[0].Title);
            Assert.Contains("staff.tumblr.com/rss", handler.Requests.Single().RequestUri!.ToString());
        }

        private static HttpResponseMessage RssResponse(string xml) =>
            new(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/rss+xml")
            };

        private static TumblrNewsSourceFetcher CreateFetcher(
            FakeHttpMessageHandler handler,
            string consumerKey)
        {
            var client = new HttpClient(handler);
            var logger = new Mock<ILogger<TumblrNewsSourceFetcher>>().Object;
            var options = Options.Create(new TumblrSettings { ConsumerKey = consumerKey });
            return new TumblrNewsSourceFetcher(client, logger, options);
        }
    }
}
