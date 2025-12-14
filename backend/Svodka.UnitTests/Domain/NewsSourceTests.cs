using Xunit;
using Svodka.Domain.Entities;

namespace Svodka.UnitTests.Domain
{
    public class NewsSourceTests
    {
        [Fact]
        public void NewsSource_Creation_Should_Set_Properties()
        {
            // Arrange
            var id = 1;
            var name = "Test RSS Source";
            var type = "rss";
            var configuration = "{\"url\":\"http://example.com/rss\",\"limit\":10}";
            var isActive = true;
            var lastPolledAtUtc = DateTime.UtcNow;
            var lastError = "No error";

            // Act
            var newsSource = new NewsSource
            {
                Id = id,
                Name = name,
                Type = type,
                Configuration = configuration,
                IsActive = isActive,
                LastPolledAtUtc = lastPolledAtUtc,
                LastError = lastError
            };

            // Assert
            Assert.Equal(id, newsSource.Id);
            Assert.Equal(name, newsSource.Name);
            Assert.Equal(type, newsSource.Type);
            Assert.Equal(configuration, newsSource.Configuration);
            Assert.Equal(isActive, newsSource.IsActive);
            Assert.Equal(lastPolledAtUtc, newsSource.LastPolledAtUtc);
            Assert.Equal(lastError, newsSource.LastError);
        }
    }
}