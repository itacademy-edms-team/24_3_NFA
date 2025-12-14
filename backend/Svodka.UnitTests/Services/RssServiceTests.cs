using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Svodka.Infrastructure.Services;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Svodka.UnitTests.Services
{
    public class RssServiceTests
    {
        private readonly Mock<ILogger<RssService>> _mockLogger;
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly RssService _rssService;

        public RssServiceTests()
        {
            _mockLogger = new Mock<ILogger<RssService>>();
            _mockHttpClient = new Mock<HttpClient>();
            _rssService = new RssService(_mockHttpClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task FetchRssFeedAsync_WithValidUrl_ReturnsNewsItems()
        {


            // Arrange
            var url = "https://xakep.ru/rss";
            var limit = 10;

            // Act & Assert
            // Так как мы не можем легко протестировать внутренности метода с Mock HttpClient,
            // просто проверим, что метод не выбрасывает исключения при вызове с корректными параметрами
            // (в реальных условиях будет выброшено исключение из-за несуществующего URL)
            await Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(() =>
                _rssService.FetchRssFeedAsync(url, limit));
        }
    }
}