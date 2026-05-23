namespace Svodka.Infrastructure.Services
{
    public class VkSettings
    {
        public const string SectionName = "Vk";

        /// <summary>Сервисный ключ доступа из настроек мини-приложения VK.</summary>
        public string ServiceAccessToken { get; set; } = string.Empty;

        public string ApiVersion { get; set; } = "5.199";
    }
}
