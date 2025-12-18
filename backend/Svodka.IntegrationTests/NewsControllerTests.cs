using Xunit;
using System.Net;
using System.Text.Json;
using Svodka.Domain.Entities;

namespace Svodka.IntegrationTests
{
    public class NewsControllerTests : IntegrationTestBase
    {
        [Fact]
        public async Task GetLatestNews_ReturnsSuccess()
        {
            // Act
            var response = await _httpClient.GetAsync("/api/news");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLatestNews_WithParameters_ReturnsSuccess()
        {
            // Act
            var response = await _httpClient.GetAsync("/api/news?offset=0&limit=10&period=week");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLatestNews_ReturnsNewsItems()
        {
            // Act
            var response = await _httpClient.GetAsync("/api/news");

            // Assert
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            // Создаем класс для десериализации ответа
            var result = JsonSerializer.Deserialize<ApiResponse>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.NotNull(result.Items);
        }

        // Внутренний класс для десериализации
        internal class ApiResponse
        {
            public IEnumerable<NewsItem> Items { get; set; }
            public bool HasMore { get; set; }
            public int Offset { get; set; }
            public int Limit { get; set; }
        }
    }
}