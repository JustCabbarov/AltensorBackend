using Altensorcrm.Domain.Enums;

namespace Altensorcrm.Contract.DTOs.Common;

public record LeadFilterDto(
    LeadStatus? Status,
    IndustryType? Industry,
    Guid? TerritoryId,
    Guid? OwnerId,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 10
);

public record DealFilterDto(
    DealStatus? Status,
    IndustryType? Industry,
    Guid? TerritoryId,
    Guid? OwnerId,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 10
);

public record ContactFilterDto(
    string? Company,
    string? Phone,
    string? Email,
    Guid? TerritoryId,
    IndustryType? Industry,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 10
);

public record OrganizationFilterDto(
    string? Company,
    Guid? TerritoryId,
    IndustryType? Industry,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 10
);

public record ConvertLeadToDealDto(
    decimal DealAmount = 0,
    Guid? AssignedUserId = null
);

public record EmployeeMetricDto(
    Guid UserId,
    string EmployeeName,
    int TotalDeals,
    decimal TotalRevenue
);

public record MonthlyRevenueDataDto(
    string Month,
    decimal Revenue,
    int DealCount
);

public record ConversionStatsDto(
    double ConversionRatePercent,
    decimal TargetAmount,
    decimal AchievedAmount,
    double TargetProgressPercent
);

public record DashboardStatsDto(
    int TotalLeads,
    double AverageTimeToCloseDays,
    int OngoingDealsCount,
    int WonDealsCount,
    decimal TotalRevenueGenerated,
    Dictionary<string, int> LostDealsByReason,
    List<EmployeeMetricDto> PerEmployeeMetrics,
    List<MonthlyRevenueDataDto>? MonthlyRevenue = null,
    ConversionStatsDto? ConversionStats = null
);
