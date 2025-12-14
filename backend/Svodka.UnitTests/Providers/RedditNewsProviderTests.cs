using Xunit;
using Moq;
using System.Net.Http;
using Svodka.Domain.Entities;
using Svodka.Domain.Models;
using Svodka.Infrastructure.Providers;
using Svodka.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Svodka.UnitTests.Providers
{
    public class RedditNewsProviderTests
    {
        private readonly Mock<ILogger<RedditNewsProvider>> _mockLogger;
        private readonly Mock<IRedditService> _mockRedditService;
        private readonly RedditNewsProvider _provider;

        public RedditNewsProviderTests()
        {
            _mockLogger = new Mock<ILogger<RedditNewsProvider>>();
            _mockRedditService = new Mock<IRedditService>();

            _provider = new RedditNewsProvider(_mockRedditService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetNewsAsync_WithValidConfiguration_ReturnsNewsItems()
        {
            // Arrange
            var config = new RedditSourceConfiguration
            {
                Subreddit = "programming",
                SortType = "hot",
                Limit = 10
            };

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Test Post", SourceItemId = "1" }
            };

            _mockRedditService.Setup(s => s.FetchSubredditPostsAsync(
                config.Subreddit,
                config.SortType,
                config.Limit))
                .ReturnsAsync(newsItems);

            // Act
            var result = await _provider.GetNewsAsync(config);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<NewsItem>>(result);
            _mockRedditService.Verify(s => s.FetchSubredditPostsAsync(
                config.Subreddit,
                config.SortType,
                config.Limit), Times.Once);
        }

        [Fact]
        public async Task GetNewsAsync_WithCategory_AssignsCategoryToNewsItems()
        {
            // Arrange
            var config = new RedditSourceConfiguration
            {
                Subreddit = "programming",
                SortType = "hot",
                Limit = 10,
                Category = "Technology"
            };

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Test Post", SourceItemId = "1", Category = null }
            };

            _mockRedditService.Setup(s => s.FetchSubredditPostsAsync(
                config.Subreddit,
                config.SortType,
                config.Limit))
                .ReturnsAsync(newsItems);

            // Act
            var result = await _provider.GetNewsAsync(config);

            // Assert
            var resultList = result.ToList();
            Assert.All(resultList, item => Assert.Equal("Technology", item.Category));
        }

        [Fact]
        public async Task GetNewsAsync_WithInvalidConfiguration_ThrowsArgumentException()
        {
            // Arrange
            var invalidConfig = new { InvalidField = "test" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _provider.GetNewsAsync(invalidConfig));
        }

        [Fact]
        public async Task GetNewsAsync_WhenRedditServiceThrowsException_PropagatesException()
        {
            // Arrange
            var config = new RedditSourceConfiguration
            {
                Subreddit = "programming",
                SortType = "hot",
                Limit = 10
            };

            _mockRedditService.Setup(s => s.FetchSubredditPostsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _provider.GetNewsAsync(config));
        }
    }
}

