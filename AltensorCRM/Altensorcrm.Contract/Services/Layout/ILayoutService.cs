using Altensorcrm.Contract.DTOs.Layout;

namespace Altensorcrm.Contract.Services.Layout;

public interface ILayoutService
{
    Task<LayoutDto> GetByModuleAsync(string moduleName, CancellationToken cancellationToken = default);
    Task<LayoutDto> UpdateByModuleAsync(string moduleName, UpdateLayoutDto dto, CancellationToken cancellationToken = default);
}
