using System;
using System.Threading;
using Altensorcrm.Contract.DTOs.Webhook;
using Altensorcrm.Contract.Services.UserManagement;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;
using Microsoft.Extensions.Logging;

namespace Altensorcrm.Application.Services.UserManagement;

public class UserSyncService : IUserSyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserSyncService> _logger;

    public UserSyncService(IUnitOfWork unitOfWork, ILogger<UserSyncService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task EnsureUserExistsAsync(UserCreatedWebhookPayload payload, CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(
            payload.UserId,
            payload.TenantId,
            payload.Email,
            payload.FullName,
            payload.UserName,
            payload.Role,
            payload.Department,
            cancellationToken);
    }

    public async System.Threading.Tasks.Task EnsureUserExistsAsync(
        Guid userId,
        Guid tenantId,
        string email,
        string? fullName,
        string? userName = null,
        string? role = null,
        string? department = null,
        CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var existing = await userRepo.GetByIdAsync(userId, cancellationToken);

        var firstName = string.Empty;
        var lastName = string.Empty;
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            var parts = fullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            firstName = parts.Length > 0 ? parts[0] : string.Empty;
            lastName = parts.Length > 1 ? parts[1] : string.Empty;
        }

        var resolvedUserName = !string.IsNullOrWhiteSpace(userName) ? userName.Trim() : email.Trim();

        if (existing is null)
        {
            _logger.LogInformation("Creating synchronized user {UserId} for Tenant {TenantId}", userId, tenantId);
            var newUser = new User
            {
                Id = userId,
                TenantId = tenantId,
                Email = email.Trim(),
                Username = resolvedUserName,
                FirstName = firstName,
                LastName = lastName,
                Role = role ?? "User",
                Department = department,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await userRepo.AddAsync(newUser, cancellationToken);
        }
        else
        {
            _logger.LogInformation("Updating synchronized user {UserId} for Tenant {TenantId}", userId, tenantId);
            existing.TenantId = tenantId;
            existing.Email = email.Trim();
            if (!string.IsNullOrWhiteSpace(resolvedUserName)) existing.Username = resolvedUserName;
            if (!string.IsNullOrWhiteSpace(firstName)) existing.FirstName = firstName;
            if (!string.IsNullOrWhiteSpace(lastName)) existing.LastName = lastName;
            if (!string.IsNullOrWhiteSpace(role)) existing.Role = role;
            if (!string.IsNullOrWhiteSpace(department)) existing.Department = department;
            existing.UpdatedAt = DateTime.UtcNow;

            userRepo.Update(existing);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

