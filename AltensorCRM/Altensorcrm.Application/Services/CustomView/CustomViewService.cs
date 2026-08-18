using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altensorcrm.Contract.DTOs.CustomView;
using Altensorcrm.Contract.Services.CustomView;
using Altensorcrm.Contract.Services.Tenant;

namespace Altensorcrm.Application.Services.CustomView;

public class CustomViewService : ICustomViewService
{
    private static readonly List<CustomViewDto> Store = new();
    private static readonly object LockObj = new();
    private readonly ICurrentTenantService _tenantService;

    public CustomViewService(ICurrentTenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public System.Threading.Tasks.Task<IReadOnlyList<CustomViewDto>> GetByModuleAsync(string moduleName, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId;
        lock (LockObj)
        {
            var query = Store.Where(v => string.Equals(v.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));
            if (!_tenantService.IsPlatformSuperAdmin && tenantId.HasValue)
            {
                query = query.Where(v => v.TenantId == tenantId.Value);
            }

            return System.Threading.Tasks.Task.FromResult<IReadOnlyList<CustomViewDto>>(query.ToList());
        }
    }

    public System.Threading.Tasks.Task<CustomViewDto> CreateAsync(CreateCustomViewDto dto, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId ?? Guid.Empty;
        var view = new CustomViewDto
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ModuleName = dto.ModuleName,
            ViewName = dto.ViewName,
            ViewType = dto.ViewType,
            ConfigJson = dto.ConfigJson
        };

        lock (LockObj)
        {
            Store.Add(view);
        }

        return System.Threading.Tasks.Task.FromResult(view);
    }

    public System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId;
        lock (LockObj)
        {
            var item = Store.FirstOrDefault(v => v.Id == id && (_tenantService.IsPlatformSuperAdmin || !tenantId.HasValue || v.TenantId == tenantId.Value));
            if (item is null) return System.Threading.Tasks.Task.FromResult(false);
            Store.Remove(item);
            return System.Threading.Tasks.Task.FromResult(true);
        }
    }
}

