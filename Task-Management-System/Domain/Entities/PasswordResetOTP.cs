using System;

namespace Domain.Entities
{
    /// <summary>
    /// Auth Service tərəfindən idarə olunur.
    /// Bu modul yalnız oxuma məqsədilə saxlayır.
    /// </summary>
    public class PasswordResetOTP
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Code { get; set; } = default!;
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; } = false;

        public AppUser User { get; set; } = default!;
    }
}
