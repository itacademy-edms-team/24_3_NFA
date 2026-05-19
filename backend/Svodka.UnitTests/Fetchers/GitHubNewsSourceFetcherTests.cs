using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
using Svodka.Infrastructure.Fetchers;
using Svodka.UnitTests.Helpers;

namespace Svodka.UnitTests.Fetchers
{
    public class GitHubNewsSourceFetcherTests
    {
        [Fact]
        public async Task FetchAsync_WithValidConfiguration_ReturnsNewsItems()
        {
            var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("""
                [
                  {
                    "id": "event-1",
                    "type": "PushEvent",
                    "created_at": "2024-04-24T12:00:00Z",
                    "actor": { "login": "octocat", "avatar_url": "https://example.com/avatar.png" },
                    "repo": { "name": "microsoft/vscode" },
                    "payload": {}
                  }
                ]
                """));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.GitHub,
                Configuration = """{"RepositoryOwner":"microsoft","RepositoryName":"vscode","Limit":10}"""
            };

            var result = (await fetcher.FetchAsync(source, 20)).ToList();

            Assert.Single(result);
            Assert.Equal("event-1", result[0].SourceItemId);
            Assert.Equal("PushEvent", result[0].Category);
            Assert.Contains("/repos/microsoft/vscode/events?per_page=10", handler.Requests.Single().RequestUri!.ToString());
        }

        [Fact]
        public async Task FetchAsync_WhenLimitIsZero_UsesDefaultLimit()
        {
            var handler = new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("[]"));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.GitHub,
                Configuration = """{"RepositoryOwner":"microsoft","RepositoryName":"vscode","Limit":0}"""
            };

            await fetcher.FetchAsync(source, 15);

            Assert.Contains("per_page=15", handler.Requests.Single().RequestUri!.ToString());
        }

        [Fact]
        public void ValidateAndNormalize_WithInvalidConfiguration_ThrowsArgumentException()
        {
            var fetcher = CreateFetcher(new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("[]")));
            using var document = System.Text.Json.JsonDocument.Parse("""{"repositoryOwner":"microsoft"}""");

            Assert.Throws<ArgumentException>(() => fetcher.ValidateAndNormalize(document.RootElement));
        }

        [Fact]
        public void GetSuggestedTags_WithCamelCaseCategory_ReturnsCategory()
        {
            var fetcher = CreateFetcher(new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("[]")));

            var result = fetcher.GetSuggestedTags("""{"category":"Technology"}""");

            Assert.Equal(new[] { "Technology" }, result);
        }

        [Fact]
        public async Task FetchAsync_WhenGitHubReturnsError_PropagatesException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var fetcher = CreateFetcher(handler);
            var source = new NewsSource
            {
                Type = SourceType.GitHub,
                Configuration = """{"RepositoryOwner":"microsoft","RepositoryName":"vscode","Limit":10}"""
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => fetcher.FetchAsync(source, 10));
        }

        private static GitHubNewsSourceFetcher CreateFetcher(HttpMessageHandler handler)
        {
            return new GitHubNewsSourceFetcher(
                new HttpClient(handler),
                Mock.Of<ILogger<GitHubNewsSourceFetcher>>());
        }
    }
}
