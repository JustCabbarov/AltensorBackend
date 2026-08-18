namespace AltensorAuthService.Contract.Users
{
    public class CreateUserRequest
    {
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string Password { get; set; } = default!;
        public List<Guid> RoleIds { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public List<Guid>? RoleIds { get; set; }
    }

    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class AssignRoleRequest
    {
        public Guid RoleId { get; set; }
    }
}
