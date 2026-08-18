using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.Setting;
using Altensorcrm.Contract.Services.Setting;
using Altensorcrm.Contract.Services.Tenant;

namespace Altensorcrm.Application.Services.Setting;

public class SettingService : ISettingService
{
    private static readonly ConcurrentDictionary<Guid, SystemSettingDto> TenantSettings = new();
    private readonly ICurrentTenantService _tenantService;

    public SettingService(ICurrentTenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public System.Threading.Tasks.Task<SystemSettingDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId ?? Guid.Empty;
        var settings = TenantSettings.GetOrAdd(tenantId, _ => new SystemSettingDto());
        return System.Threading.Tasks.Task.FromResult(settings);
    }

    public System.Threading.Tasks.Task<SystemSettingDto> UpdateSettingsAsync(SystemSettingDto dto, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId ?? Guid.Empty;
        TenantSettings[tenantId] = dto;
        return System.Threading.Tasks.Task.FromResult(dto);
    }
}

