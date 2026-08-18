using Altensorcrm.Contract.DTOs.CallLog;

namespace Altensorcrm.Contract.Services.CallLog;

public interface ICallLogService
{
    Task<IReadOnlyList<CallLogDetailDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CallLogDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CallLogDetailDto> CreateAsync(CreateCallLogDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
