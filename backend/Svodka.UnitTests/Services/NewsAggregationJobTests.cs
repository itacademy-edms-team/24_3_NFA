using Xunit;
using Moq;
using Svodka.Domain.Interfaces;
using Svodka.Domain.Entities;
using Svodka.Domain.Enums;
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
        private readonly Mock<INewsSourceFetcherFactory> _mockFetcherFactory;
        private readonly Mock<INewsSourceRepository> _mockSourceRepository;
        private readonly Mock<INewsItemRepository> _mockItemRepository;
        private readonly NewsAggregatorDbContext _dbContext;
        private readonly NewsAggregationOptions _options;

        public NewsAggregationJobTests()
        {
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockLogger = new Mock<ILogger<NewsAggregationJob>>();
            _mockScope = new Mock<IServiceScope>();
            _mockFetcherFactory = new Mock<INewsSourceFetcherFactory>();
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
                .Setup(sp => sp.GetService(typeof(INewsSourceFetcherFactory)))
                .Returns(_mockFetcherFactory.Object);
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
                Type = SourceType.Rss,
                Configuration = "{\"url\":\"http://example.com\",\"limit\":10}",
                IsActive = true
            };

            var fetcher = new Mock<INewsSourceFetcher>();
            fetcher
                .Setup(f => f.FetchAsync(source, _options.NewsLimitPerSource, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NewsItem>
                {
                    new NewsItem { Title = "Test News", SourceItemId = "1" }
                });

            _mockSourceRepository.Setup(r => r.GetByIdAsync(sourceId)).ReturnsAsync(source);
            _mockFetcherFactory.Setup(f => f.GetFetcher(source.Type)).Returns(fetcher.Object);
            _mockItemRepository.Setup(r => r.SaveNewsAsync(It.IsAny<IEnumerable<NewsItem>>())).Returns(Task.CompletedTask);
            _mockSourceRepository.Setup(r => r.UpdateLastPolledAtAsync(sourceId, It.IsAny<DateTime>())).Returns(Task.CompletedTask);
            _mockSourceRepository.Setup(r => r.ClearLastErrorAsync(sourceId)).Returns(Task.CompletedTask);

            var job = new NewsAggregationJob(
                _mockScopeFactory.Object,
                _mockLogger.Object,
                Options.Create(_options)
            );

            // Act
            await job.ExecuteAsync(sourceId, cancellationToken: CancellationToken.None);

            // Assert
            _mockSourceRepository.Verify(r => r.GetByIdAsync(sourceId), Times.Once);
            _mockFetcherFactory.Verify(f => f.GetFetcher(source.Type), Times.Once);
            fetcher.Verify(f => f.FetchAsync(source, _options.NewsLimitPerSource, It.IsAny<CancellationToken>()), Times.Once);
            _mockItemRepository.Verify(r => r.SaveNewsAsync(It.Is<IEnumerable<NewsItem>>(items =>
                items.Single().SourceId == sourceId)), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithNoSourceId_CallsProviderForAllActiveSources()
        {
            // Arrange
            var sources = new List<NewsSource>
            {
                new NewsSource { Id = 1, Name = "Test Source 1", Type = SourceType.Rss, Configuration = "{\"url\":\"http://example.com\",\"limit\":10}", IsActive = true },
                new NewsSource { Id = 2, Name = "Test Source 2", Type = SourceType.Rss, Configuration = "{\"url\":\"http://example.com\",\"limit\":10}", IsActive = true }
            };

            _mockSourceRepository.Setup(r => r.GetActiveNewsSourcesAsync()).ReturnsAsync(sources);
            _mockFetcherFactory
                .Setup(f => f.GetFetcher(It.IsAny<SourceType>()))
                .Returns((SourceType _) =>
                {
                    var fetcher = new Mock<INewsSourceFetcher>();
                    fetcher
                        .Setup(f => f.FetchAsync(It.IsAny<NewsSource>(), _options.NewsLimitPerSource, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<NewsItem>
                        {
                            new NewsItem { Title = "Test News", SourceItemId = Guid.NewGuid().ToString() }
                        });
                    return fetcher.Object;
                });
            _mockItemRepository.Setup(r => r.SaveNewsAsync(It.IsAny<IEnumerable<NewsItem>>())).Returns(Task.CompletedTask);
            _mockSourceRepository.Setup(r => r.UpdateLastPolledAtAsync(It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
            _mockSourceRepository.Setup(r => r.ClearLastErrorAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var job = new NewsAggregationJob(
                _mockScopeFactory.Object,
                _mockLogger.Object,
                Options.Create(_options)
            );

            // Act
            await job.ExecuteAsync(cancellationToken: CancellationToken.None);

            // Assert
            _mockSourceRepository.Verify(r => r.GetActiveNewsSourcesAsync(), Times.Once);
            _mockFetcherFactory.Verify(f => f.GetFetcher(SourceType.Rss), Times.Exactly(2));
            _mockItemRepository.Verify(r => r.SaveNewsAsync(It.IsAny<IEnumerable<NewsItem>>()), Times.Exactly(2));
        }
    }
}
