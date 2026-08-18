namespace AltensorAuthService.Application.Interfaces
{
    public interface ICurrentTenantService
    {
        Guid? TenantId { get; }
        string? TenantStatus { get; }
        Guid? UserId { get; }
        bool IsAuthenticated { get; }
        bool IsPlatformSuperAdmin { get; }
        bool IsTenantAdmin { get; }
    }
}
