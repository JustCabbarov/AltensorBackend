using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public Guid? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public bool IsSystemRole { get; set; } = false; // true isə tenant onu silə/redaktə edə bilməz

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
