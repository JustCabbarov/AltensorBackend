namespace AltensorAuthService.Contract.Permissions
{
    public class PermissionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid ModuleId { get; set; }
        public string ModuleCode { get; set; } = default!;
    }
}
