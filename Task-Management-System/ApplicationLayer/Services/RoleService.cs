using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Exceptions;
using Domain.Entities;

using EvTap.Contracts.DTOs;
using EvTap.Contracts.Services;
using Microsoft.AspNetCore.Identity;

namespace EvTap.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }



       
        public async Task<bool> AssignRoleAsync(AssignRoleDTO DTO)
        {
            if (DTO == null)
                throw new NotNullExceptions("Can not be null");
            var user = await _userManager.FindByIdAsync(DTO.UserId);
            if (user == null)
                throw new NotFoundException("User tapılmadı.");

            if (!await _roleManager.RoleExistsAsync(DTO.RoleName))
                throw new NotFoundException("Role mövcud deyil.");

            var result = await _userManager.AddToRoleAsync(user, DTO.RoleName);
            return result.Succeeded;
        }

    }
}
