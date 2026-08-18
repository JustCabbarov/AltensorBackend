using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Presentation.Controllers.Internal
{
    /// <summary>
    /// Auth Service-in bu modula göndərdiyi daxili webhook-lar.
    /// Bu endpoint-lər ictimai API deyil, sistemlər arası inteqrasiya üçündür.
    /// </summary>
    [ApiController]
    [Route("internal/webhooks")]
    [AllowAnonymous] // Auth Service Bearer token daşımır — X-Webhook-Secret ilə qorunur
    public class AuthWebhookController : ControllerBase
    {
        private readonly IAppUserRepository _appUserRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthWebhookController> _logger;

        public AuthWebhookController(
            IAppUserRepository appUserRepository,
            IConfiguration configuration,
            ILogger<AuthWebhookController> logger)
        {
            _appUserRepository = appUserRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Auth Service yeni istifadəçi və ya tenant yaradanda bu endpoint-ə POST edir.
        /// TMS lokal AppUsers cədvəlini sinxronizasiya edir.
        /// POST /internal/webhooks/user-created
        /// </summary>
        [HttpPost("user-created")]
        public async Task<IActionResult> OnUserCreated([FromBody] UserCreatedWebhookPayload payload)
        {
            // 1. Shared secret yoxlanışı
            if (!IsValidSecret())
            {
                _logger.LogWarning("Webhook sorğusu rədd edildi: Yanlış və ya çatışmayan Secret açar.");
                return Unauthorized(new { error = "Yanlış və ya çatışmayan webhook secret." });
            }

            if (payload == null)
            {
                _logger.LogWarning("Webhook xətası: Payload boşdur.");
                return BadRequest(new { error = "Payload boş ola bilməz." });
            }

            try
            {
                _logger.LogInformation("Yeni istifadəçi sinxronizasiya edilir: UserId={UserId}, TenantId={TenantId}, Email='{Email}'",
                    payload.UserId, payload.TenantId, payload.Email);

                // 2. Lokal bazada tenant və istifadəçini təmin et (yarat və ya yenilə)
                await _appUserRepository.EnsureExistsAsync(
                    payload.UserId,
                    payload.TenantId,
                    payload.Email,
                    payload.FullName,
                    payload.UserName);

                _logger.LogInformation("İstifadəçi uğurla sinxronizasiya edildi: UserId={UserId}", payload.UserId);

                return Ok(new { message = "User uğurla sinxronizasiya edildi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstifadəçi sinxronizasiya edilərkən xəta baş verdi: UserId={UserId}", payload.UserId);
                return StatusCode(500, new { error = "Sinxronizasiya zamanı daxili xəta baş verdi.", details = ex.Message });
            }
        }

        private bool IsValidSecret()
        {
            var expectedSecret = _configuration["Webhook:SharedSecret"]
                              ?? _configuration["InternalCommunication:ApiKey"];

            // Əgər config-də heç nə yazılmayıbsa (Dev mühiti üçün buraxır)
            if (string.IsNullOrEmpty(expectedSecret))
                return true;

            // X-Webhook-Secret və ya X-Internal-Api-Key başlıqlarını yoxla
            if (Request.Headers.TryGetValue("X-Webhook-Secret", out var receivedSecret) ||
                Request.Headers.TryGetValue("X-Internal-Api-Key", out receivedSecret))
            {
                return string.Equals(receivedSecret.ToString(), expectedSecret, StringComparison.Ordinal);
            }

            return false;
        }
    }

    public class UserCreatedWebhookPayload
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
