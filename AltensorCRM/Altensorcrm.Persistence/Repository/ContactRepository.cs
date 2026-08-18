using System;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;
using Altensorcrm.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Altensorcrm.Persistence.Repository;

public class ContactRepository : GenericRepository<Contact>, IContactRepository
{
    public ContactRepository(AppDbContext context, ICurrentTenantService tenantService)
        : base(context, tenantService)
    {
    }

    public async Task<Contact?> GetContactWithDetailsByIdAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        return await ApplyTenantFilter(DbSet.AsNoTracking())
            .Include(c => c.Address)
            .Include(c => c.Organization)
            .Include(c => c.AssignedUser)
            .Include(c => c.Deals)
            .FirstOrDefaultAsync(c => c.Id == contactId, cancellationToken);
    }
}

