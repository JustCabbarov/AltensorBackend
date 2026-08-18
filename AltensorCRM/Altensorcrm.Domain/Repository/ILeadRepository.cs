using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Repository;

public interface ILeadRepository : IGenericRepository<Lead>
{
    Task<Lead?> GetLeadWithDetailsByIdAsync(Guid leadId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Lead> Items, int TotalCount)> GetFilteredLeadsAsync(string? searchTerm, LeadStatus? status, Guid? ownerId, int page, int pageSize, CancellationToken cancellationToken = default);
}
