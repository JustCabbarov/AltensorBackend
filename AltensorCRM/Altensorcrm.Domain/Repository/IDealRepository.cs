using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Domain.Repository;

public interface IDealRepository : IGenericRepository<Deal>
{
    Task<Deal?> GetDealWithDetailsByIdAsync(Guid dealId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Deal>> GetDealsByStageAsync(DealStatus status, CancellationToken cancellationToken = default);
}
