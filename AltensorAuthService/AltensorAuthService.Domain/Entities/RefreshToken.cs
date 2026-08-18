using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!;

        public string TokenHash { get; set; } = default!; // xam tokeni saxlama, yalnız hash

        public DateTime ExpiresAt { get; set; }

        public bool Revoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }

        public string? DeviceInfo { get; set; } // audit/təhlükəsizlik üçün faydalı (browser, IP və s.)


        public bool IsActive => !Revoked && ExpiresAt > DateTime.UtcNow;
    }
}
