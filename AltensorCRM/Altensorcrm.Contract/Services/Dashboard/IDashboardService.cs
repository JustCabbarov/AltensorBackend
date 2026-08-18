using Altensorcrm.Contract.DTOs.Common;

namespace Altensorcrm.Contract.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}
