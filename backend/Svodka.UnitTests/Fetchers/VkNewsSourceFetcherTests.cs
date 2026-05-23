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
    public class VkNewsSourceFetcherTests
    {
        [Fact]
        public async Task FetchAsync_WithDomain_BuildsWallGetUrlPerDocs()
        {
            string? capturedQuery = null;
            var handler = new FakeHttpMessageHandler(req =>
            {
                if (req.RequestUri!.AbsolutePath.Contains("wall.get"))
                {
                    capturedQuery = req.RequestUri.Query;
                    return FakeHttpMessageHandler.Json("""
                        {
                          "response": {
                            "items": [
                              {
                                "id": 42,
                                "owner_id": -1,
                                "date": 1713945600,
                                "text": "Тестовая запись VK"
                              }
                            ]
                          }
                        }
                        """);
                }
                return FakeHttpMessageHandler.Json("{}");
            });

            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Vk,
                Configuration = """{"domain":"kinoluch","limit":5,"filter":"all"}"""
            };

            var result = (await fetcher.FetchAsync(source, 10)).ToList();

            Assert.NotNull(capturedQuery);
            Assert.Contains("domain=kinoluch", capturedQuery);
            Assert.Contains("filter=all", capturedQuery);
            Assert.Contains("offset=0", capturedQuery);
            Assert.Contains("count=5", capturedQuery);
            Assert.Contains("v=5.199", capturedQuery);
            Assert.DoesNotContain("owner_id=", capturedQuery);
            Assert.Single(result);
            Assert.Contains("wall-1_42", result[0].Link);
        }

        [Fact]
        public async Task FetchAsync_WithOwnerId_UsesOwnerIdParam()
        {
            string? capturedQuery = null;
            var handler = new FakeHttpMessageHandler(req =>
            {
                if (req.RequestUri!.AbsolutePath.Contains("wall.get"))
                {
                    capturedQuery = req.RequestUri.Query;
                    return FakeHttpMessageHandler.Json("""
                        {
                          "response": {
                            "items": [
                              { "id": 1, "owner_id": -99, "date": 1713945600, "text": "Post" }
                            ]
                          }
                        }
                        """);
                }
                return FakeHttpMessageHandler.Json("{}");
            });

            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.Vk,
                Configuration = """{"ownerId":-99,"limit":10}"""
            };

            await fetcher.FetchAsync(source, 20);

            Assert.Contains("owner_id=-99", capturedQuery);
            Assert.Contains("filter=all", capturedQuery);
        }

        private static VkNewsSourceFetcher CreateFetcher(FakeHttpMessageHandler handler)
        {
            var client = new HttpClient(handler);
            var logger = new Mock<ILogger<VkNewsSourceFetcher>>().Object;
            var options = Options.Create(new VkSettings { ServiceAccessToken = "test-token", ApiVersion = "5.199" });
            return new VkNewsSourceFetcher(client, logger, options);
        }
    }
}
