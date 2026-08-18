using System;

namespace Altensorcrm.Contract.DTOs.Webhook;

public class UserCreatedWebhookPayload
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string Email { get; set; } = default!;
    public string? FullName { get; set; }
    public string? UserName { get; set; }
    public string? Role { get; set; }
    public string? Department { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

