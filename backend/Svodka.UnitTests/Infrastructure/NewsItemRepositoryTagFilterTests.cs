using Xunit;
using Svodka.Domain.Entities;
using Svodka.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Svodka.Infrastructure.Data;

namespace Svodka.UnitTests.Infrastructure
{
    public class NewsItemRepositoryTagFilterTests : IDisposable
    {
        private readonly NewsAggregatorDbContext _context;
        private readonly NewsItemRepository _repository;

        public NewsItemRepositoryTagFilterTests()
        {
            var options = new DbContextOptionsBuilder<NewsAggregatorDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new NewsAggregatorDbContext(options);
            _repository = new NewsItemRepository(_context);
        }

        [Fact]
        public async Task GetLatestNewsAsync_MultipleTags_AndLogic_ShouldReturnItemsWithAllTags()
        {
            // Arrange
            var tag1 = new Tag { Id = 1, Name = "Tag1", NormalizedName = "tag1" };
            var tag2 = new Tag { Id = 2, Name = "Tag2", NormalizedName = "tag2" };
            await _context.Tags.AddRangeAsync(tag1, tag2);

            var sourceBoth = new NewsSource { Id = 1, Name = "Source Both", UserId = 1 };
            var sourceOnly1 = new NewsSource { Id = 2, Name = "Source Only 1", UserId = 1 };
            await _context.NewsSources.AddRangeAsync(sourceBoth, sourceOnly1);

            await _context.NewsSourceTags.AddRangeAsync(
                new NewsSourceTag { NewsSourceId = 1, TagId = 1, Tag = tag1 },
                new NewsSourceTag { NewsSourceId = 1, TagId = 2, Tag = tag2 },
                new NewsSourceTag { NewsSourceId = 2, TagId = 1, Tag = tag1 }
            );

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "News from Both", SourceId = 1, PublishedAtUtc = DateTime.UtcNow },
                new NewsItem { Id = 2, Title = "News from Only 1", SourceId = 2, PublishedAtUtc = DateTime.UtcNow.AddMinutes(-1) }
            };

            await _context.NewsItems.AddRangeAsync(newsItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestNewsAsync(
                limit: 10,
                tags: new List<string> { "Tag1", "Tag2" }
            );

            // Assert
            var resultList = result.ToList();
            // With AND logic, only news from sourceBoth (id=1) should be returned
            Assert.Single(resultList);
            Assert.Equal(1, resultList[0].SourceId);
        }

        [Fact]
        public async Task GetLatestNewsAsync_SingleTag_ShouldReturnItemsWithThatTag()
        {
            // Arrange
            var tag1 = new Tag { Id = 1, Name = "Tag1", NormalizedName = "tag1" };
            await _context.Tags.AddAsync(tag1);

            var source1 = new NewsSource { Id = 1, Name = "Source 1", UserId = 1 };
            var source2 = new NewsSource { Id = 2, Name = "Source 2", UserId = 1 };
            await _context.NewsSources.AddRangeAsync(source1, source2);

            await _context.NewsSourceTags.AddAsync(new NewsSourceTag { NewsSourceId = 1, TagId = 1, Tag = tag1 });

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "News 1", SourceId = 1, PublishedAtUtc = DateTime.UtcNow },
                new NewsItem { Id = 2, Title = "News 2", SourceId = 2, PublishedAtUtc = DateTime.UtcNow.AddMinutes(-1) }
            };

            await _context.NewsItems.AddRangeAsync(newsItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestNewsAsync(
                limit: 10,
                tags: new List<string> { "Tag1" }
            );

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(1, resultList[0].SourceId);
        }

        [Fact]
        public async Task GetLatestNewsAsync_TagFromSourceConfigurationCategory_ShouldReturnItemsFromThatSource()
        {
            // Arrange
            var source = new NewsSource
            {
                Id = 1,
                Name = "Source with Category",
                UserId = 1,
                Configuration = """{"category":"Technology"}"""
            };
            var otherSource = new NewsSource
            {
                Id = 2,
                Name = "Source without Category",
                UserId = 1,
                Configuration = """{"category":"World"}"""
            };
            await _context.NewsSources.AddRangeAsync(source, otherSource);

            var newsItems = new List<NewsItem>
            {
                new NewsItem { Id = 1, Title = "Tech News", SourceId = 1, PublishedAtUtc = DateTime.UtcNow },
                new NewsItem { Id = 2, Title = "World News", SourceId = 2, PublishedAtUtc = DateTime.UtcNow.AddMinutes(-1) }
            };

            await _context.NewsItems.AddRangeAsync(newsItems);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLatestNewsAsync(
                limit: 10,
                tags: new List<string> { "Technology" }
            );

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal(1, resultList[0].SourceId);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
