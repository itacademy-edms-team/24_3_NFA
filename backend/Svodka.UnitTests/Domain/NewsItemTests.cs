using Xunit;
using Svodka.Domain.Entities;

namespace Svodka.UnitTests.Domain
{
    public class NewsItemTests
    {
        [Fact]
        public void NewsItem_Creation_Should_Set_Properties()
        {
            // Arrange
            var id = 1;
            var title = "Test News";
            var description = "Test Description";
            var link = "http://example.com";
            var publishedAtUtc = DateTime.UtcNow;
            var sourceId = 1;
            var sourceItemId = "source_item_id";
            var author = "John Doe";
            var imageUrl = "http://example.com/image.jpg";
            var category = "Technology";
            var indexedAtUtc = DateTime.UtcNow;

            // Act
            var newsItem = new NewsItem
            {
                Id = id,
                Title = title,
                Description = description,
                Link = link,
                PublishedAtUtc = publishedAtUtc,
                SourceId = sourceId,
                SourceItemId = sourceItemId,
                Author = author,
                ImageUrl = imageUrl,
                Category = category,
                IndexedAtUtc = indexedAtUtc
            };

            // Assert
            Assert.Equal(id, newsItem.Id);
            Assert.Equal(title, newsItem.Title);
            Assert.Equal(description, newsItem.Description);
            Assert.Equal(link, newsItem.Link);
            Assert.Equal(publishedAtUtc, newsItem.PublishedAtUtc);
            Assert.Equal(sourceId, newsItem.SourceId);
            Assert.Equal(sourceItemId, newsItem.SourceItemId);
            Assert.Equal(author, newsItem.Author);
            Assert.Equal(imageUrl, newsItem.ImageUrl);
            Assert.Equal(category, newsItem.Category);
            Assert.Equal(indexedAtUtc, newsItem.IndexedAtUtc);
        }
    }
}