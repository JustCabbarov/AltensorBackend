using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.DTOs.Contact;

namespace Altensorcrm.Contract.Services.Contact;

public interface IContactService
{
    Task<ContactDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ContactListDto>> GetPagedListAsync(ContactFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactListDto>> GetLookupAsync(CancellationToken cancellationToken = default);
    Task<ContactDetailDto> CreateAsync(CreateContactDto dto, CancellationToken cancellationToken = default);
    Task<ContactDetailDto> UpdateAsync(UpdateContactDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
