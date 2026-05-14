namespace Svodka.Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailConfirmationAsync(string toEmail, string confirmationLink);
        Task<bool> SendPasswordResetAsync(string toEmail, string resetLink);
    }
}
