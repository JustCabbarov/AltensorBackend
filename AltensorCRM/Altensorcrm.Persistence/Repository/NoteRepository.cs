using Altensorcrm.Contract.Services.Tenant;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Repository;
using Altensorcrm.Persistence.Data;

namespace Altensorcrm.Persistence.Repository;

public class NoteRepository(AppDbContext context, ICurrentTenantService tenantService)
    : GenericRepository<Note>(context, tenantService), INoteRepository
{
}

