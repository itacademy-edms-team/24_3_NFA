using Xunit;
using Moq;
using Svodka.Domain.Entities;
using Svodka.Domain.Interfaces;
using Svodka.Infrastructure.Providers;
using Svodka.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Svodka.Domain.Models;

namespace Svodka.UnitTests.Providers
{
    public class RssNewsProviderTests
    {
        private readonly Mock<ILogger<RssNewsProvider>> _mockLogger;
        private readonly Mock<IRssService> _mockRssService;
        private readonly RssNewsProvider _provider;

        public RssNewsProviderTests()
        {
            _mockLogger = new Mock<ILogger<RssNewsProvider>>();
            _mockRssService = new Mock<IRssService>();

            _provider = new RssNewsProvider(_mockRssService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetNewsAsync_WithValidConfiguration_ReturnsNewsItems()
        {
            // Arrange
            var config = new RssSourceConfiguration
            {
                Url = "http://example.com/rss",
                Limit = 10
            };

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Test News", SourceItemId = "1" }
            };

            _mockRssService.Setup(s => s.FetchRssFeedAsync(config.Url, config.Limit))
                           .ReturnsAsync(newsItems);

            // Act
            var result = await _provider.GetNewsAsync(config);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<NewsItem>>(result);
            _mockRssService.Verify(s => s.FetchRssFeedAsync(config.Url, config.Limit), Times.Once);
        }

        [Fact]
        public async Task GetNewsAsync_WithCategory_AssignsCategoryToNewsItems()
        {
            // Arrange
            var config = new RssSourceConfiguration
            {
                Url = "http://example.com/rss",
                Limit = 10,
                Category = "Technology"
            };

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Test News", SourceItemId = "1", Category = null }
            };

            _mockRssService.Setup(s => s.FetchRssFeedAsync(config.Url, config.Limit))
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
        public async Task GetNewsAsync_WhenRssServiceThrowsException_PropagatesException()
        {
            // Arrange
            var config = new RssSourceConfiguration
            {
                Url = "http://example.com/rss",
                Limit = 10
            };

            _mockRssService.Setup(s => s.FetchRssFeedAsync(config.Url, config.Limit))
                           .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _provider.GetNewsAsync(config));
        }
    }
}