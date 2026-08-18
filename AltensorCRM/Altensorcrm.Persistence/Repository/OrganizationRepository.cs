using System;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;
using Altensorcrm.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Altensorcrm.Persistence.Repository;

public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(AppDbContext context, ICurrentTenantService tenantService)
        : base(context, tenantService)
    {
    }

    public async Task<Organization?> GetOrganizationWithDetailsByIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await ApplyTenantFilter(DbSet.AsNoTracking())
            .Include(o => o.Territory)
            .Include(o => o.Address)
            .Include(o => o.Contacts)
            .Include(o => o.Deals)
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
    }
}

