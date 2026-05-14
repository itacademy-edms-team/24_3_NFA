using Svodka.Application.DTOs;

namespace Svodka.Application.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResultDto> RegisterAsync(string email, string password);
        Task<string?> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<object?> GetUserProfileAsync(int userId);
        Task<bool> ConfirmEmailAsync(string token);
        Task ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task ResendConfirmationAsync(string email);
    }
}
