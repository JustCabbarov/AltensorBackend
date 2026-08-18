using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvTap.Contracts.DTOs
{
    public record AssignRoleDTO
    {
        public string UserId { get; set; }
        public string RoleName { get; set; }
    }

}
