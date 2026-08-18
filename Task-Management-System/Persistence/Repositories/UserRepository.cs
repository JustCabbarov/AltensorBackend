using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;

namespace Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<AppUser>> GetAllUsersAsync(Guid tenantId)
        {
            return await _db.AppUsers
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId)
                .Include(u => u.PerformancePoints)
                .ToListAsync();
        }

        public async Task<AppUser?> GetByIdAsync(Guid userId)
        {
            return await _db.AppUsers.FindAsync(userId);
        }
    }
}
