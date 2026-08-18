using AltensorAuthService.Application.Interfaces;
using AltensorAuthService.Contract.Auth;
using AltensorAuthService.Contract.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AltensorAuthService.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(
            IAuthService authService,
            ICurrentTenantService currentTenantService,
            IPasswordResetService passwordResetService)
        {
            _authService = authService;
            _currentTenantService = currentTenantService;
            _passwordResetService = passwordResetService;
        }

        /// <summary>
        /// Email + şifrə + tenantSlug ilə sistemə daxil olmaq və RS256 imzalı JWT almaq
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Refresh token ilə yeni access token və yeni refresh token almaq (Token Rotation)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var response = await _authService.RefreshAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Cari cihazdan çıxış (Refresh token revoke olunur)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest? request)
        {
            await _authService.LogoutAsync(request?.RefreshToken);
            return Ok(new { message = "Çıxış uğurla tamamlandı." });
        }

        /// <summary>
        /// Bütün cihazlardan çıxış (bütün refresh token-lər ləğv edilir)
        /// </summary>
        [Authorize]
        [HttpPost("logout-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LogoutAll()
        {
            if (_currentTenantService.UserId.HasValue)
            {
                await _authService.LogoutAllDevicesAsync(_currentTenantService.UserId.Value);
            }
            return Ok(new { message = "Bütün cihazlardan çıxış uğurla icra olundu." });
        }

        /// <summary>
        /// Cari daxil olmuş istifadəçi, onun tenant-ı, rolları, icazələri və aktiv modulları
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMe()
        {
            var userInfo = await _authService.GetCurrentUserInfoAsync();
            return Ok(userInfo);
        }

        /// <summary>
        /// OTP kodu email-ə göndərmək (şifrə sıfırlama üçün)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _passwordResetService.ForgotPasswordAsync(request.Email, request.TenantSlug);
            return Ok(new { message = "Əgər hesab mövcuddursa, şifrə sıfırlama kodu email-ə göndərildi." });
        }

        /// <summary>
        /// OTP kodu ilə şifrəni sıfırlamaq
        /// </summary>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _passwordResetService.ResetPasswordAsync(
                request.Email, request.TenantSlug, request.Otp, request.NewPassword);
            return Ok(new { message = "Şifrə uğurla sıfırlandı." });
        }
    }
}
