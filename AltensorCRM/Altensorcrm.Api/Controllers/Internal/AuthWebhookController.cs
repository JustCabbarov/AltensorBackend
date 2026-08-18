using System;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.Webhook;
using Altensorcrm.Contract.Services.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Altensorcrm.Api.Controllers.Internal;

[ApiController]
[Route("internal/webhooks")]
public class AuthWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserSyncService _userSyncService;
    private readonly ILogger<AuthWebhookController> _logger;

    public AuthWebhookController(
        IConfiguration configuration,
        IUserSyncService userSyncService,
        ILogger<AuthWebhookController> logger)
    {
        _configuration = configuration;
        _userSyncService = userSyncService;
        _logger = logger;
    }

    /// <summary>
    /// Auth Service POSTs here when a user is created or updated in the platform.
    /// Auth Service calls: POST /internal/webhooks/user-created
    /// </summary>
    [HttpPost("user-created")]
    [AllowAnonymous]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OnUserCreated(
        [FromBody] UserCreatedWebhookPayload payload,
        [FromHeader(Name = "X-Webhook-Secret")] string? webhookSecret,
        [FromHeader(Name = "X-Internal-Api-Key")] string? apiKey,
        CancellationToken cancellationToken)
    {
        // 1. Təhlükəsizlik yoxlanışı (Security verification)
        var expectedSecret = _configuration["Webhook:SharedSecret"] 
                          ?? _configuration["InternalCommunication:ApiKey"];

        var incomingSecret = !string.IsNullOrWhiteSpace(webhookSecret) ? webhookSecret : apiKey;

        if (string.IsNullOrWhiteSpace(expectedSecret) || incomingSecret != expectedSecret)
        {
            _logger.LogWarning("İcazəsiz webhook sorğusu qeydə alındı. UserId: {UserId}", payload?.UserId);
            return Unauthorized(new { message = "Unauthorized webhook request." });
        }

        if (payload == null || payload.UserId == Guid.Empty || payload.TenantId == Guid.Empty)
        {
            return BadRequest(new { message = "Invalid event payload." });
        }

        // 2. Sinxronizasiya icrası (Idempotent execution)
        _logger.LogInformation("Webhook qəbul edildi: UserId={UserId}, TenantId={TenantId}, Email={Email}", 
            payload.UserId, payload.TenantId, payload.Email);

        await _userSyncService.EnsureUserExistsAsync(payload, cancellationToken);

        return Ok(new { success = true, message = "User synchronized successfully.", userId = payload.UserId, tenantId = payload.TenantId });
    }
}

