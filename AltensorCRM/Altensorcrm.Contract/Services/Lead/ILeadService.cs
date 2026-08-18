using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Deal;
using Altensorcrm.Contract.DTOs.Lead;

namespace Altensorcrm.Contract.Services.Lead;

public interface ILeadService
{
    Task<LeadDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<LeadListDto>> GetPagedListAsync(LeadFilterDto filter, CancellationToken cancellationToken = default);
    Task<LeadDetailDto> CreateAsync(CreateLeadDto dto, CancellationToken cancellationToken = default);
    Task<LeadDetailDto> UpdateAsync(UpdateLeadDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DealDetailDto> ConvertLeadToDealAsync(Guid leadId, ConvertLeadToDealDto dto, CancellationToken cancellationToken = default);
}
