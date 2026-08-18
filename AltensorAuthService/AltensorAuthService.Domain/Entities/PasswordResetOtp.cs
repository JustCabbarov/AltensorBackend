namespace AltensorAuthService.Domain.Entities
{
    public class PasswordResetOtp : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        public string Code { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public bool IsValid => !IsUsed && ExpiresAt > DateTime.UtcNow;
    }
}
