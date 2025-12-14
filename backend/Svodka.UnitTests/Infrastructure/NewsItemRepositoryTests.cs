using Xunit;
using Svodka.Domain.Entities;
using Svodka.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Svodka.Infrastructure.Data;

namespace Svodka.UnitTests.Infrastructure
{
    public class NewsItemRepositoryTests : IDisposable
    {
        private readonly NewsAggregatorDbContext _context;
        private readonly NewsItemRepository _repository;

        public NewsItemRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<NewsAggregatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Уникальная база для каждого теста
                .Options;

            _context = new NewsAggregatorDbContext(options);
            _repository = new NewsItemRepository(_context);
        }

        [Fact]
        public async Task GetLatestNewsAsync_WithValidParams_ReturnsNewsItems()
        {
            // Arrange
            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Test News 1", PublishedAtUtc = DateTime.UtcNow.AddDays(-1) },
                new NewsItem { Id = 2, Title = "Test News 2", PublishedAtUtc = DateTime.UtcNow }
            };

            await _context.NewsItems.AddRangeAsync(newsItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestNewsAsync(
                limit: 10,
                searchQuery: null,
                fromDateUtc: DateTime.UtcNow.AddDays(-7),
                sourceIds: null,
                categories: null,
                offset: 0,
                sourceType: null
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task SaveNewsAsync_WithValidNews_ShouldAddNewsToDatabase()
        {
            // Arrange
            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Test News 1", PublishedAtUtc = DateTime.UtcNow, SourceId = 1, SourceItemId = "1" }
            };

            // Act
            await _repository.SaveNewsAsync(newsItems);

            // Assert
            var savedItems = await _context.NewsItems.ToListAsync();
            Assert.Single(savedItems);
            Assert.Equal("Test News 1", savedItems.First().Title);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}