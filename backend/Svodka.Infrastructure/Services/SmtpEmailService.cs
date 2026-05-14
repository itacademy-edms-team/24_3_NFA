using MailKit.Net.Smtp;

using MailKit.Security;

using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Options;

using MimeKit;

using Svodka.Application.Interfaces;



namespace Svodka.Infrastructure.Services

{

    public class SmtpEmailService : IEmailService

    {

        private readonly SmtpSettings _settings;

        private readonly ILogger<SmtpEmailService> _logger;



        public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)

        {

            _settings = settings.Value;

            _logger = logger;

        }



        public Task<bool> SendEmailConfirmationAsync(string toEmail, string confirmationLink) =>

            SendAsync(toEmail, "Подтверждение регистрации в Сводке",

                $"Здравствуйте!\n\nДля завершения регистрации перейдите по ссылке:\n{confirmationLink}\n\nСсылка действительна 24 часа.");



        public Task<bool> SendPasswordResetAsync(string toEmail, string resetLink) =>

            SendAsync(toEmail, "Сброс пароля в Сводке",

                $"Здравствуйте!\n\nДля сброса пароля перейдите по ссылке:\n{resetLink}\n\nСсылка действительна 1 час.");



        private async Task<bool> SendAsync(string toEmail, string subject, string body)

        {

            if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.UserName))

            {

                _logger.LogWarning(

                    "SMTP отключён или не настроен. Письмо для {Email} не отправлено. Тема: {Subject}",

                    toEmail,

                    subject);

                return false;

            }



            try

            {

                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));

                message.To.Add(MailboxAddress.Parse(toEmail));

                message.Subject = subject;

                message.Body = new TextPart("plain") { Text = body };



                using var client = new SmtpClient();

                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect);

                await client.AuthenticateAsync(_settings.UserName, _settings.Password);

                await client.SendAsync(message);

                await client.DisconnectAsync(true);



                _logger.LogInformation("Письмо отправлено на {Email}", toEmail);

                return true;

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Ошибка отправки письма на {Email}. Тема: {Subject}", toEmail, subject);

                return false;

            }

        }

    }

}


