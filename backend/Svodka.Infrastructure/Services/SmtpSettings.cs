namespace Svodka.Infrastructure.Services
{
    public class SmtpSettings
    {
        public const string SectionName = "SmtpSettings";

        public string Host { get; set; } = "smtp.yandex.ru";
        public int Port { get; set; } = 465;
        public bool UseSsl { get; set; } = true;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "Сводка";
        public bool Enabled { get; set; } = true;
    }
}
