using Domain.Entities;
using System;
using System.Collections.Generic;

namespace Domain.Repositories
{
    public interface IUserRepository
    {
        Task<List<AppUser>> GetAllUsersAsync(Guid tenantId);
        Task<AppUser?> GetByIdAsync(Guid userId);
    }
}
