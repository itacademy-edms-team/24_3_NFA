using Xunit;
using Svodka.Domain.Entities;
using Svodka.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Svodka.Infrastructure.Data;

namespace Svodka.UnitTests.Infrastructure
{
    public class NewsSourceRepositoryTests : IDisposable
    {
        private readonly NewsAggregatorDbContext _context;
        private readonly NewsSourceRepository _repository;

        public NewsSourceRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<NewsAggregatorDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            _context = new NewsAggregatorDbContext(options);
            _repository = new NewsSourceRepository(_context);
        }

        [Fact]
        public async Task AddNewsSourceAsync_WithValidSource_ShouldAddSourceToDatabase()
        {
            // Arrange
            var newsSource = new NewsSource
            {
                Id = 1,
                Name = "Test Source",
                Type = "rss",
                Configuration = "{\"url\":\"http://example.com\",\"limit\":10}",
                IsActive = true
            };

            // Act
            await _repository.AddNewsSourceAsync(newsSource);
            await _context.SaveChangesAsync();

            // Assert
            var savedSource = await _context.NewsSources.FindAsync(1);
            Assert.NotNull(savedSource);
            Assert.Equal("Test Source", savedSource.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsSource()
        {
            // Arrange
            var newsSource = new NewsSource
            {
                Id = 1,
                Name = "Test Source",
                Type = "rss",
                Configuration = "{\"url\":\"http://example.com\",\"limit\":10}",
                IsActive = true
            };

            await _context.NewsSources.AddAsync(newsSource);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newsSource.Id, result.Id);
        }

        [Fact]
        public async Task GetAllSourcesAsync_ReturnsAllSources()
        {
            // Arrange
            var newsSources = new List<NewsSource>
            {
                new NewsSource { Id = 1, Name = "Test Source 1", Type = "rss", Configuration = "{}", IsActive = true },
                new NewsSource { Id = 2, Name = "Test Source 2", Type = "rss", Configuration = "{}", IsActive = true }
            };

            await _context.NewsSources.AddRangeAsync(newsSources);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllSourcesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}