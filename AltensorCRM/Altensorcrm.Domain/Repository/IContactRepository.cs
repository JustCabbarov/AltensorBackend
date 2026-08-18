using Altensorcrm.Domain.Entity;

namespace Altensorcrm.Domain.Repository;

public interface IContactRepository : IGenericRepository<Contact>
{
    Task<Contact?> GetContactWithDetailsByIdAsync(Guid contactId, CancellationToken cancellationToken = default);
}
