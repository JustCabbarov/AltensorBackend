using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltensorAuthService.Domain.Entities
{
    public class Permission : BaseEntity
    {

        public string Code { get; set; } = default!; // "inventory.read", "inventory.approve_transfer"

        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        public Guid ModuleId { get; set; } // hansı modula aiddir
        public SistemModule Module { get; set; } = default!;

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
