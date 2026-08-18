using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvTap.Contracts.DTOs;

namespace EvTap.Contracts.Services
{
    public interface IRoleService
    {
     
        Task<bool> AssignRoleAsync(AssignRoleDTO dTO);
    }
}
