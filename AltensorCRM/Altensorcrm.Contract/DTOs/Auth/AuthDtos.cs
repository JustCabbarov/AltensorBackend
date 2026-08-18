namespace Altensorcrm.Contract.DTOs.Auth;

public record LoginRequestDto(
    string Username,
    string Password
);

public record LoginResponseDto(
    string Token,
    DateTime Expiration,
    Guid UserId,
    string Username,
    string FullName,
    string Email,
    string Role,
    string? Department
);

public record RegisterUserDto(
    string Username,
    string Password,
    string FirstName,
    string LastName,
    string Email,
    string? Department
);

public record ChangePasswordDto(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
);
