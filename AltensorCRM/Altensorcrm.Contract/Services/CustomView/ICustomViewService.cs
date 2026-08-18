using Altensorcrm.Contract.DTOs.CustomView;

namespace Altensorcrm.Contract.Services.CustomView;

public interface ICustomViewService
{
    Task<IReadOnlyList<CustomViewDto>> GetByModuleAsync(string moduleName, CancellationToken cancellationToken = default);
    Task<CustomViewDto> CreateAsync(CreateCustomViewDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
