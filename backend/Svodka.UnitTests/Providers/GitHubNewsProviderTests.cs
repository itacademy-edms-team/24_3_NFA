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
    public class GitHubNewsProviderTests
    {
        private readonly Mock<ILogger<GitHubNewsProvider>> _mockLogger;
        private readonly Mock<IGitHubService> _mockGitHubService;
        private readonly GitHubNewsProvider _provider;

        public GitHubNewsProviderTests()
        {
            _mockLogger = new Mock<ILogger<GitHubNewsProvider>>();
            _mockGitHubService = new Mock<IGitHubService>();

            _provider = new GitHubNewsProvider(_mockGitHubService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetNewsAsync_WithValidConfiguration_ReturnsNewsItems()
        {
            // Arrange
            var config = new GitHubSourceConfiguration
            {
                RepositoryOwner = "microsoft",
                RepositoryName = "vscode",
                Limit = 10
            };

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Test Event", SourceItemId = "1" }
            };

            _mockGitHubService.Setup(s => s.FetchRepositoryEventsAsync(
                config.RepositoryOwner,
                config.RepositoryName,
                config.Token,
                config.Limit,
                config.EventTypes))
                .ReturnsAsync(newsItems);

            // Act
            var result = await _provider.GetNewsAsync(config);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<NewsItem>>(result);
            _mockGitHubService.Verify(s => s.FetchRepositoryEventsAsync(
                config.RepositoryOwner,
                config.RepositoryName,
                config.Token,
                config.Limit,
                config.EventTypes), Times.Once);
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
        public async Task GetNewsAsync_WhenGitHubServiceThrowsException_PropagatesException()
        {
            // Arrange
            var config = new GitHubSourceConfiguration
            {
                RepositoryOwner = "microsoft",
                RepositoryName = "vscode",
                Limit = 10
            };

            _mockGitHubService.Setup(s => s.FetchRepositoryEventsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<List<string>?>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                _provider.GetNewsAsync(config));
        }
    }
}

