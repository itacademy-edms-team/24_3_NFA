using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Svodka.Application.DTOs;
using Svodka.Application.Interfaces;
using System.Security.Claims;

namespace Svodka.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto.Email, dto.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _authService.LoginAsync(dto.Email, dto.Password);
                if (token == null)
                {
                    return Unauthorized(new { message = "Неверный email или пароль" });
                }
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Токен не указан" });
            }

            var ok = await _authService.ConfirmEmailAsync(token);
            if (!ok)
            {
                return BadRequest(new { message = "Недействительная или устаревшая ссылка подтверждения" });
            }

            return Ok(new { message = "Email успешно подтверждён. Теперь можно войти." });
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto dto)
        {
            try
            {
                await _authService.ResendConfirmationAsync(dto.Email);
                return Ok(new { message = "Если email зарегистрирован и не подтверждён, на него отправлена новая ссылка." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "Если email зарегистрирован, на него отправлена ссылка для сброса пароля." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var ok = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!ok)
            {
                return BadRequest(new { message = "Недействительная или устаревшая ссылка сброса пароля" });
            }
            return Ok(new { message = "Пароль успешно изменён" });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var profile = await _authService.GetUserProfileAsync(userId);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _authService.ChangePasswordAsync(userId, dto.OldPassword, dto.NewPassword);
            if (!result)
            {
                return BadRequest(new { message = "Не удалось сменить пароль. Проверьте старый пароль." });
            }
            return Ok(new { message = "Пароль успешно изменен" });
        }
    }
}
