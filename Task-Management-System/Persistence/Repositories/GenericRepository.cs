using Contract.Services;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System;
using System.Collections.Generic;

namespace Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity, new()
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly ICurrentTenantService _tenantService;

        public GenericRepository(AppDbContext context, ICurrentTenantService tenantService)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _tenantService = tenantService;
        }

        private Guid GetRequiredTenantId()
        {
            return _tenantService.TenantId
                ?? throw new UnauthorizedAccessException("Tenant konteksti tapılmadı.");
        }

        public async Task<T> AddAsync(T entity)
        {
            entity.TenantId = GetRequiredTenantId(); // ← həmişə tenant set et
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<T> DeleteAsync(Guid id)
        {
            var tenantId = GetRequiredTenantId();
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
            if (entity == null)
                throw new Exception("Entity tapılmadı və ya bu tenanta aid deyil.");

            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return entity;
        }

        public async Task<List<T>> GetAllAsync(Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            var tenantId = GetRequiredTenantId();

            IQueryable<T> query = _dbSet
                .AsNoTracking()
                .Where(e => e.TenantId == tenantId && !e.IsDeleted); // ← tenant filter

            if (include != null)
                query = include(query);

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            var tenantId = GetRequiredTenantId();

            IQueryable<T> query = _dbSet
                .Where(e => e.Id == id && e.TenantId == tenantId && !e.IsDeleted); // ← tenant filter

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var tenantId = GetRequiredTenantId();
            var existingEntity = await _dbSet.FirstOrDefaultAsync(e => e.Id == entity.Id && e.TenantId == tenantId);
            if (existingEntity == null)
                throw new Exception("Entity tapılmadı və ya bu tenanta aid deyil.");

            entity.UpdatedAt = DateTime.UtcNow;
            entity.TenantId = tenantId; // tenant dəyişdirilə bilməz
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            return existingEntity;
        }
    }
}
