using Altensorcrm.Domain.Entity;

namespace Altensorcrm.Domain.Repository;

public interface IOrganizationRepository : IGenericRepository<Organization>
{
    Task<Organization?> GetOrganizationWithDetailsByIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
