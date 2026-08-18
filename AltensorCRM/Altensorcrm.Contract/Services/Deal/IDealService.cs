using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Deal;
using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Contract.Services.Deal;

public interface IDealService
{
    Task<DealDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DealListDto>> GetPagedListAsync(DealFilterDto filter, CancellationToken cancellationToken = default);
    Task<DealDetailDto> CreateAsync(CreateDealDto dto, CancellationToken cancellationToken = default);
    Task<DealDetailDto> UpdateAsync(UpdateDealDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateStageAsync(Guid dealId, DealStatus newStatus, string? lostReason, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
