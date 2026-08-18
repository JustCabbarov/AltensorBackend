namespace AltensorAuthService.Contract.Roles
{
    public class CreateRoleRequest
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public List<Guid> PermissionIds { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public List<Guid> PermissionIds { get; set; } = new();
    }

    public class RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
        public bool IsSystemRole { get; set; }
        public List<string> Permissions { get; set; } = new();
        public List<Guid> PermissionIds { get; set; } = new();
    }
}
