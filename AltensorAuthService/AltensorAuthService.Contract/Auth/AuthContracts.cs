namespace AltensorAuthService.Contract.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string TenantSlug { get; set; } = default!;
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = default!;
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = default!;
        public string TenantSlug { get; set; } = default!;
        public string TenantStatus { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public List<string> Modules { get; set; } = new();
    }

    public class JwksKeyDto
    {
        public string Kty { get; set; } = "RSA";
        public string Use { get; set; } = "sig";
        public string Alg { get; set; } = "RS256";
        public string Kid { get; set; } = "altensor-auth-key-1";
        public string N { get; set; } = default!;
        public string E { get; set; } = default!;
    }

    public class JwksDto
    {
        public List<JwksKeyDto> Keys { get; set; } = new();
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = default!;
        public string TenantSlug { get; set; } = default!;
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = default!;
        public string TenantSlug { get; set; } = default!;
        public string Otp { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}
