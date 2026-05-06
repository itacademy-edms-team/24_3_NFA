using System.Text.Json;
using System.Text.Json.Serialization;

namespace Svodka.Domain.Models
{
    public static class SourceConfigurationJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static T Deserialize<T>(JsonElement json) where T : class
        {
            var result = JsonSerializer.Deserialize<T>(json.GetRawText(), Options);
            if (result == null)
            {
                throw new ArgumentException("Некорректная конфигурация источника.");
            }
            return result;
        }

        public static string Serialize<T>(T config) => JsonSerializer.Serialize(config, Options);

        public static void ValidateLimit(int limit, int min = 1, int max = 100)
        {
            if (limit < min || limit > max)
            {
                throw new ArgumentException($"Лимит новостей должен быть от {min} до {max}.");
            }
        }
    }
}
