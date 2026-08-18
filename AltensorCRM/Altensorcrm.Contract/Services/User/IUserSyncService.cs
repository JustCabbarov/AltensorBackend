using System;
using System.Threading;
using Altensorcrm.Contract.DTOs.Webhook;

namespace Altensorcrm.Contract.Services.UserManagement;

public interface IUserSyncService
{
    System.Threading.Tasks.Task EnsureUserExistsAsync(UserCreatedWebhookPayload payload, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task EnsureUserExistsAsync(Guid userId, Guid tenantId, string email, string? fullName, string? userName = null, string? role = null, string? department = null, CancellationToken cancellationToken = default);
}

