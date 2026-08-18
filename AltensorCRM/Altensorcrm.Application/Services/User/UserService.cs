using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.UserManagement;
using Altensorcrm.Contract.Services.UserManagement;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;

namespace Altensorcrm.Application.Services.UserManagement
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var users = await userRepo.GetAllAsync(cancellationToken);

            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(id, cancellationToken);
            if (user == null) return null;

            return MapToDto(user);
        }

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            var userRepo = _unitOfWork.Repository<User>();
            var users = await userRepo.FindAsync(u => u.Email == email.Trim(), cancellationToken);
            var user = users.FirstOrDefault();
            if (user == null) return null;

            return MapToDto(user);
        }

        public async Task<UserDto?> UpdateProfileAsync(Guid id, UpdateUserProfileDto dto, CancellationToken cancellationToken = default)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(id, cancellationToken);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
            {
                user.FirstName = dto.FirstName.Trim();
            }
            if (!string.IsNullOrWhiteSpace(dto.LastName))
            {
                user.LastName = dto.LastName.Trim();
            }
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                var parts = dto.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                user.FirstName = parts.Length > 0 ? parts[0] : user.FirstName;
                user.LastName = parts.Length > 1 ? parts[1] : (parts.Length > 0 ? "" : user.LastName);
            }
            user.UpdatedAt = DateTime.UtcNow;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(user);
        }

        public async Task<string?> UpdateAvatarAsync(Guid id, string avatarUrl, CancellationToken cancellationToken = default)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(id, cancellationToken);
            if (user == null) return null;

            user.AvatarUrl = avatarUrl;
            user.UpdatedAt = DateTime.UtcNow;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return avatarUrl;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userRepo = _unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(id, cancellationToken);
            if (user is null) return false;

            userRepo.Delete(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<object> GetSalesHierarchyAsync(CancellationToken cancellationToken = default)
        {
            var users = await GetAllAsync(cancellationToken);
            return new
            {
                tree = users.Select(u => new
                {
                    id = u.Id,
                    name = u.Name,
                    role = u.Role,
                    isManager = u.IsManager
                })
            };
        }

        private static UserDto MapToDto(User u)
        {
            var fullName = $"{u.FirstName} {u.LastName}".Trim();
            var roleName = string.IsNullOrWhiteSpace(u.Role) ? "User" : u.Role;

            return new UserDto
            {
                Id = u.Id,
                Name = string.IsNullOrWhiteSpace(fullName) ? (u.Username.Length > 0 ? u.Username : u.Email) : fullName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = roleName,
                AvatarUrl = u.AvatarUrl,
                IsManager = roleName.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                            roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                            roleName.Equals("TenantAdmin", StringComparison.OrdinalIgnoreCase) ||
                            roleName.Equals("PlatformSuperAdmin", StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
