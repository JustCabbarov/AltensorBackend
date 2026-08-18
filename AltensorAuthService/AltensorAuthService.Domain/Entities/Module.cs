using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Domain.Entities
{
    public class SistemModule :BaseEntity
    {
      

        public string Code { get; set; } = default!; // "crm", "inventory", "hr"

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true; // sistemdə hazırda satışa açıq olub-olmadığı

        public ICollection<TenantModuleSubscription> Subscriptions { get; set; } = new List<TenantModuleSubscription>();
        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
