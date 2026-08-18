using Altensorcrm.Contract.DTOs.Setting;

namespace Altensorcrm.Contract.Services.Setting;

public interface ISettingService
{
    Task<SystemSettingDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<SystemSettingDto> UpdateSettingsAsync(SystemSettingDto dto, CancellationToken cancellationToken = default);
}
