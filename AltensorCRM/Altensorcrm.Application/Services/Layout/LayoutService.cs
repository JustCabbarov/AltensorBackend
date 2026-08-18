using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.Layout;
using Altensorcrm.Contract.Services.Layout;
using Altensorcrm.Contract.Services.Tenant;

namespace Altensorcrm.Application.Services.Layout;

public class LayoutService : ILayoutService
{
    private static readonly ConcurrentDictionary<string, string> Store = new(StringComparer.OrdinalIgnoreCase);
    private readonly ICurrentTenantService _tenantService;

    public LayoutService(ICurrentTenantService tenantService)
    {
        _tenantService = tenantService;
    }

    private string GetKey(string moduleName)
    {
        var tenantId = _tenantService.TenantId?.ToString() ?? "global";
        return $"{tenantId}_{moduleName}";
    }

    public System.Threading.Tasks.Task<LayoutDto> GetByModuleAsync(string moduleName, CancellationToken cancellationToken = default)
    {
        var key = GetKey(moduleName);
        Store.TryGetValue(key, out var json);
        return System.Threading.Tasks.Task.FromResult(new LayoutDto
        {
            ModuleName = moduleName,
            LayoutJson = json ?? "[]"
        });
    }

    public System.Threading.Tasks.Task<LayoutDto> UpdateByModuleAsync(string moduleName, UpdateLayoutDto dto, CancellationToken cancellationToken = default)
    {
        var key = GetKey(moduleName);
        Store[key] = dto.LayoutJson;
        return System.Threading.Tasks.Task.FromResult(new LayoutDto
        {
            ModuleName = moduleName,
            LayoutJson = dto.LayoutJson
        });
    }
}

