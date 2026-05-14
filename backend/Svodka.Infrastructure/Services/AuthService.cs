using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Svodka.Application.DTOs;
using Svodka.Application.Interfaces;
using Svodka.Domain.Entities;
using Svodka.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Svodka.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly NewsAggregatorDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly SmtpSettings _smtpSettings;

        public AuthService(
            NewsAggregatorDbContext context,
            IConfiguration configuration,
            IEmailService emailService,
            Microsoft.Extensions.Options.IOptions<SmtpSettings> smtpOptions)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _smtpSettings = smtpOptions.Value;
        }

        private bool IsSmtpConfigured =>
            _smtpSettings.Enabled && !string.IsNullOrWhiteSpace(_smtpSettings.UserName);

        public async Task<RegisterResultDto> RegisterAsync(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                throw new Exception("Пользователь с таким email уже существует");
            }

            var confirmationToken = GenerateSecureToken();
            var smtpConfigured = IsSmtpConfigured;

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAtUtc = DateTime.UtcNow,
                EmailConfirmed = !smtpConfigured,
                EmailConfirmationToken = smtpConfigured ? confirmationToken : null,
                EmailConfirmationExpiresUtc = smtpConfigured ? DateTime.UtcNow.AddHours(24) : null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (smtpConfigured)
            {
                var sent = await SendConfirmationEmailAsync(user, confirmationToken);
                if (!sent)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    throw new Exception("Не удалось отправить письмо подтверждения. Проверьте настройки SMTP.");
                }

                return new RegisterResultDto
                {
                    RequiresEmailConfirmation = true,
                    Message = "На ваш email отправлена ссылка для подтверждения регистрации."
                };
            }

            return new RegisterResultDto
            {
                Token = GenerateJwtToken(user),
                RequiresEmailConfirmation = false,
                Message = "Регистрация успешна."
            };
        }

        public async Task<string?> LoginAsync(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }

            if (!user.EmailConfirmed)
            {
                throw new Exception("Подтвердите email перед входом. Проверьте почту или запросите повторную отправку.");
            }

            return GenerateJwtToken(user);
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null) return false;

            if (user.EmailConfirmationExpiresUtc.HasValue &&
                user.EmailConfirmationExpiresUtc.Value < DateTime.UtcNow)
            {
                return false;
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationExpiresUtc = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ResendConfirmationAsync(string email)
        {
            if (!IsSmtpConfigured)
            {
                throw new Exception("Отправка писем отключена. Включите SMTP в настройках.");
            }

            email = email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || user.EmailConfirmed)
            {
                return;
            }

            var confirmationToken = GenerateSecureToken();
            user.EmailConfirmationToken = confirmationToken;
            user.EmailConfirmationExpiresUtc = DateTime.UtcNow.AddHours(24);
            await _context.SaveChangesAsync();

            var sent = await SendConfirmationEmailAsync(user, confirmationToken);
            if (!sent)
            {
                throw new Exception("Не удалось отправить письмо. Проверьте настройки SMTP.");
            }
        }

        public async Task ForgotPasswordAsync(string email)
        {
            if (!IsSmtpConfigured)
            {
                return;
            }

            email = email.Trim().ToLowerInvariant();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return;

            var token = GenerateSecureToken();
            user.PasswordResetToken = token;
            user.PasswordResetExpiresUtc = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:5173";
            var link = $"{frontendUrl}/reset-password?token={token}";
            await _emailService.SendPasswordResetAsync(email, link);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == token &&
                u.PasswordResetExpiresUtc != null &&
                u.PasswordResetExpiresUtc > DateTime.UtcNow);

            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpiresUtc = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<object?> GetUserProfileAsync(int userId)
        {
            return await _context.Users
                .Select(u => new { u.Id, u.Email, u.CreatedAtUtc, u.EmailConfirmed })
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        private async Task<bool> SendConfirmationEmailAsync(User user, string confirmationToken)
        {
            // var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:5173";
            var frontendUrl = _configuration["App:FrontendUrl"] ?? "https://svodka.cloudpub.ru";
            var link = $"{frontendUrl}/confirm-email?token={confirmationToken}";
            return await _emailService.SendEmailConfirmationAsync(user.Email, link);
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SUPER_SECRET_KEY_SVODKA_2024_PROEKT");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
