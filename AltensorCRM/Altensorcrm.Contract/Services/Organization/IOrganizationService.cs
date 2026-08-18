using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Contact;
using Altensorcrm.Contract.DTOs.Deal;
using Altensorcrm.Contract.DTOs.Organization;

namespace Altensorcrm.Contract.Services.Organization;

public interface IOrganizationService
{
    Task<OrganizationDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<OrganizationListDto>> GetPagedListAsync(OrganizationFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizationListDto>> GetLookupAsync(CancellationToken cancellationToken = default);
    Task<OrganizationDetailDto> CreateAsync(CreateOrganizationDto dto, CancellationToken cancellationToken = default);
    Task<OrganizationDetailDto> UpdateAsync(UpdateOrganizationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactListDto>> GetContactsByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DealListDto>> GetDealsByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
