using System.Threading.Tasks;

namespace Svodka.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(string email, string password);
        Task<string?> LoginAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<object?> GetUserProfileAsync(int userId);
    }
}
