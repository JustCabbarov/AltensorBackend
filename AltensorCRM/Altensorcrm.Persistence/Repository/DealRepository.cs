using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Enums;
using Altensorcrm.Domain.Repository;
using Altensorcrm.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Altensorcrm.Persistence.Repository;

public class DealRepository : GenericRepository<Deal>, IDealRepository
{
    public DealRepository(AppDbContext context, ICurrentTenantService tenantService)
        : base(context, tenantService)
    {
    }

    public async Task<Deal?> GetDealWithDetailsByIdAsync(Guid dealId, CancellationToken cancellationToken = default)
    {
        return await ApplyTenantFilter(DbSet.AsNoTracking())
            .Include(d => d.SourceLead)
            .Include(d => d.Organization)
            .Include(d => d.Contact)
            .Include(d => d.DealOwner)
            .Include(d => d.Tasks)
            .Include(d => d.Notes)
            .FirstOrDefaultAsync(d => d.Id == dealId, cancellationToken);
    }

    public async Task<IReadOnlyList<Deal>> GetDealsByStageAsync(DealStatus status, CancellationToken cancellationToken = default)
    {
        return await ApplyTenantFilter(DbSet.AsNoTracking())
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

