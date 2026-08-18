using System;

namespace Altensorcrm.Contract.DTOs.UserManagement;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public string? AvatarUrl { get; set; }
    public bool IsManager { get; set; }
}

public class UpdateUserProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public class InviteUserDto
{
    public string Emails { get; set; } = string.Empty;
    public string Role { get; set; } = "Sales User";
}

public class UpdateUserRoleDto
{
    public string Role { get; set; } = "Admin";
}
