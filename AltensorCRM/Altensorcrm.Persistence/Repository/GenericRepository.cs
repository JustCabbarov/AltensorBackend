using System.Linq.Expressions;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Domain.Common;
using Altensorcrm.Domain.Repository;
using Altensorcrm.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Altensorcrm.Persistence.Repository;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    protected readonly AppDbContext Context;
    protected readonly ICurrentTenantService TenantService;
    protected readonly DbSet<TEntity> DbSet;

    public GenericRepository(AppDbContext context, ICurrentTenantService tenantService)
    {
        Context = context;
        TenantService = tenantService;
        DbSet = context.Set<TEntity>();
    }

    protected virtual IQueryable<TEntity> ApplyTenantFilter(IQueryable<TEntity> query)
    {
        if (typeof(ITenantEntity).IsAssignableFrom(typeof(TEntity)) &&
            !TenantService.IsPlatformSuperAdmin &&
            TenantService.TenantId.HasValue)
        {
            var tenantId = TenantService.TenantId.Value;
            return query.Where(e => ((ITenantEntity)e).TenantId == tenantId);
        }

        return query;
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = ApplyTenantFilter(DbSet);
        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await ApplyTenantFilter(DbSet.AsNoTracking()).ToListAsync(cancellationToken);
    }

    public virtual async Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedResponseAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = ApplyTenantFilter(DbSet.AsNoTracking());

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // Add explicit ordering to resolve Skip/Take EF Core warning
        query = query.OrderByDescending(e => EF.Property<DateTime>(e, "CreatedAt"));

        List<TEntity> items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is ITenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && TenantService.TenantId.HasValue)
        {
            tenantEntity.TenantId = TenantService.TenantId.Value;
        }

        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (TenantService.TenantId.HasValue)
        {
            var tenantId = TenantService.TenantId.Value;
            foreach (var entity in entities)
            {
                if (entity is ITenantEntity tenantEntity && tenantEntity.TenantId == Guid.Empty)
                {
                    tenantEntity.TenantId = tenantId;
                }
            }
        }

        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void DeleteRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = ApplyTenantFilter(DbSet);
        if (predicate is not null)
        {
            return await query.CountAsync(predicate, cancellationToken);
        }
        return await query.CountAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = ApplyTenantFilter(DbSet);
        return await query.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await ApplyTenantFilter(DbSet.AsNoTracking()).Where(predicate).ToListAsync(cancellationToken);
    }
}
