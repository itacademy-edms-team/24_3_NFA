using Xunit;
using Moq;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Entities;
using Svodka.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Svodka.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Svodka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Svodka.UnitTests.Services
{
    public class NewsAggregationJobTests
    {
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<ILogger<NewsAggregationJob>> _mockLogger;
        private readonly Mock<IServiceScope> _mockScope;
        private readonly IServiceProvider _mockServiceProvider;
        private readonly Mock<INewsProviderFactory> _mockProviderFactory;
        private readonly Mock<INewsSourceRepository> _mockSourceRepository;
        private readonly Mock<INewsItemRepository> _mockItemRepository;
        private readonly NewsAggregatorDbContext _dbContext;
        private readonly NewsAggregationOptions _options;

        public NewsAggregationJobTests()
        {
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockLogger = new Mock<ILogger<NewsAggregationJob>>();
            _mockScope = new Mock<IServiceScope>();
            _mockProviderFactory = new Mock<INewsProviderFactory>();
            _mockSourceRepository = new Mock<INewsSourceRepository>();
            _mockItemRepository = new Mock<INewsItemRepository>();

            _options = new NewsAggregationOptions
            {
                PollingIntervalMinutes = 5,
                NewsLimitPerSource = 10
            };

            // Создаем реальный контекст с In-Memory базой
            var options = new DbContextOptionsBuilder<NewsAggregatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new NewsAggregatorDbContext(options);

            // Создаем мок IServiceProvider с реализацией метода GetService
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(INewsProviderFactory)))
                .Returns(_mockProviderFactory.Object);
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(INewsSourceRepository)))
                .Returns(_mockSourceRepository.Object);
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(INewsItemRepository)))
                .Returns(_mockItemRepository.Object);
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(NewsAggregatorDbContext)))
                .Returns(_dbContext);

            _mockServiceProvider = mockServiceProvider.Object;

            // Настройка моков
            _mockScopeFactory.Setup(s => s.CreateScope()).Returns(_mockScope.Object);
            _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider);
        }

        [Fact]
        public async Task ExecuteAsync_WithSourceId_CallsProviderForSpecificSource()
        {
            // Arrange
            var sourceId = 1;
            var source = new NewsSource
            {
                Id = sourceId,
                Name = "Test Source",
                Type = "rss",
                Configuration = "{\"url\":\"http://example.com\",\"limit\":10}",
                IsActive = true
            };

            var config = new RssSourceConfiguration { Url = "http://example.com", Limit = 10 };

            _mockSourceRepository.Setup(r => r.GetByIdAsync(sourceId)).ReturnsAsync(source);
            _mockProviderFactory.Setup(f => f.GetProvider(source.Type)).Returns(Mock.Of<INewsProvider>());
            _mockItemRepository.Setup(r => r.SaveNewsAsync(It.IsAny<IEnumerable<NewsItem>>())).Returns(Task.CompletedTask);
            _mockSourceRepository.Setup(r => r.UpdateLastPolledAtAsync(sourceId, It.IsAny<DateTime>())).Returns(Task.CompletedTask);

            var job = new NewsAggregationJob(
                _mockScopeFactory.Object,
                _mockLogger.Object,
                Options.Create(_options)
            );

            // Act
            await job.ExecuteAsync(sourceId, CancellationToken.None);

            // Assert
            _mockSourceRepository.Verify(r => r.GetByIdAsync(sourceId), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoSourceId_CallsProviderForAllActiveSources()
        {
            // Arrange
            var sources = new List<NewsSource>
            {
                new NewsSource { Id = 1, Name = "Test Source 1", Type = "rss", Configuration = "{\"url\":\"http://example.com\",\"limit\":10}", IsActive = true },
                new NewsSource { Id = 2, Name = "Test Source 2", Type = "rss", Configuration = "{\"url\":\"http://example.com\",\"limit\":10}", IsActive = true }
            };

            _mockSourceRepository.Setup(r => r.GetActiveNewsSourcesAsync()).ReturnsAsync(sources);
            _mockProviderFactory.Setup(f => f.GetProvider(It.IsAny<string>())).Returns(Mock.Of<INewsProvider>());
            _mockItemRepository.Setup(r => r.SaveNewsAsync(It.IsAny<IEnumerable<NewsItem>>())).Returns(Task.CompletedTask);
            _mockSourceRepository.Setup(r => r.UpdateLastPolledAtAsync(It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

            var job = new NewsAggregationJob(
                _mockScopeFactory.Object,
                _mockLogger.Object,
                Options.Create(_options)
            );

            // Act
            await job.ExecuteAsync(null, CancellationToken.None);

            // Assert
            _mockSourceRepository.Verify(r => r.GetActiveNewsSourcesAsync(), Times.Once);
        }
    }
}