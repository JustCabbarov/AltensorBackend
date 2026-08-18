using AltensorAuthService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Domain.Entities
{
    public class TenantModuleSubscription: BaseEntity
    {
        

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = default!;

        public Guid ModuleId { get; set; }
        public SistemModule Module { get; set; } = default!;

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        public DateTime StartsAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public DateTime? SuspendedAt { get; set; }

        public string? SuspendReason { get; set; } // "payment_failed", "manual", və s.
    }
}
