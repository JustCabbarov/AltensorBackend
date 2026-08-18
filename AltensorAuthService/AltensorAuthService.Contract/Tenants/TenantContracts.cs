namespace AltensorAuthService.Contract.Tenants
{
    public class CreateTenantRequest
    {
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Domain { get; set; }
        public string AdminEmail { get; set; } = default!;
        public string? AdminFullName { get; set; }
        public string AdminPassword { get; set; } = default!;
        public List<Guid> ModuleIds { get; set; } = new();
    }

    public class TenantResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Domain { get; set; }
        public string Status { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime? SuspendedAt { get; set; }
    }

    public class TenantModuleSubscriptionDto
    {
        public Guid ModuleId { get; set; }
        public string ModuleCode { get; set; } = default!;
        public string ModuleName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTime StartsAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public string? SuspendReason { get; set; }
    }

    public class TenantDetailResponse : TenantResponse
    {
        public List<TenantModuleSubscriptionDto> Subscriptions { get; set; } = new();
        public int UserCount { get; set; }
    }

    public class SuspendTenantRequest
    {
        public string? Reason { get; set; }
    }

    public class ModuleSubscriptionRequest
    {
        public Guid ModuleId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class SuspendModuleRequest
    {
        public string? Reason { get; set; }
    }
}
