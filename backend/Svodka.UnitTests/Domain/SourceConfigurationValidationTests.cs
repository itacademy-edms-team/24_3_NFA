using System.Text.Json;
using Svodka.Domain.Models;
using Xunit;

namespace Svodka.UnitTests.Domain
{
    public class SourceConfigurationValidationTests
    {
        [Fact]
        public void RssValidateAndNormalize_AddsHttps_WhenSchemeMissing()
        {
            var json = JsonDocument.Parse("""{"url":"example.com/feed","limit":5}""").RootElement;
            var normalized = RssSourceConfiguration.ValidateAndNormalizeFromJson(json);
            Assert.Contains("https://example.com/feed", normalized);
        }

        [Fact]
        public void RssValidate_Throws_WhenUrlEmpty()
        {
            var json = JsonDocument.Parse("""{"url":"","limit":10}""").RootElement;
            Assert.Throws<ArgumentException>(() => RssSourceConfiguration.ValidateAndNormalizeFromJson(json));
        }

        [Fact]
        public void TumblrValidate_NormalizesBlogName()
        {
            var json = JsonDocument.Parse("""{"blogName":"My-Blog","limit":10}""").RootElement;
            var normalized = TumblrSourceConfiguration.ValidateAndNormalizeFromJson(json);
            Assert.Contains("my-blog", normalized);
        }
    }
}
