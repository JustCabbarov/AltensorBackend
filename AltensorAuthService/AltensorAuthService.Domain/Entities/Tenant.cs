using AltensorAuthService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = default!;

        public string Slug { get; set; } = default!; // login üçün: "abc-company"

        public string? Domain { get; set; } // istəyə görə subdomain, məs: "abc.yourapp.com"

        public TenantStatus Status { get; set; } = TenantStatus.Trial;

        public DateTime? SuspendedAt { get; set; }

        // Navigation properties
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
        public ICollection<TenantModuleSubscription> ModuleSubscriptions { get; set; } = new List<TenantModuleSubscription>();
    }
}
