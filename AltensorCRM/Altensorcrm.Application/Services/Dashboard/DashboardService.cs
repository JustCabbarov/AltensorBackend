using Altensorcrm.Contract.DTOs.Common;
using Altensorcrm.Contract.Services.Dashboard;
using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Enums;
using Altensorcrm.Domain.Repository;

namespace Altensorcrm.Application.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var allLeads = await _unitOfWork.Leads.GetAllAsync(cancellationToken);
        var totalLeads = allLeads.Count;

        var convertedLeads = allLeads.Where(l => l.Status == LeadStatus.ConvertToDeal).ToList();
        double avgTimeToCloseDays = 0;

        var allDeals = await _unitOfWork.Deals.GetAllAsync(cancellationToken);
        var ongoingDealsCount = allDeals.Count(d => d.Status != DealStatus.Won && d.Status != DealStatus.Lost);
        var wonDeals = allDeals.Where(d => d.Status == DealStatus.Won).ToList();
        var wonDealsCount = wonDeals.Count;
        var totalRevenueGenerated = wonDeals.Sum(d => d.AnnualRevenue);

        var lostDeals = allDeals.Where(d => d.Status == DealStatus.Lost).ToList();
        var lostDealsByReason = lostDeals
            .GroupBy(d => string.IsNullOrWhiteSpace(d.LostReason) ? "Unspecified" : d.LostReason!)
            .ToDictionary(g => g.Key, g => g.Count());

        var users = await _unitOfWork.Repository<Domain.Entity.User>().GetAllAsync(cancellationToken);
        var perEmployeeMetrics = new List<EmployeeMetricDto>();

        foreach (var user in users)
        {
            var userDeals = allDeals.Where(d => d.DealOwnerId == user.Id).ToList();
            var userWonDeals = userDeals.Where(d => d.Status == DealStatus.Won).ToList();

            perEmployeeMetrics.Add(new EmployeeMetricDto(
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim(),
                userDeals.Count,
                userWonDeals.Sum(d => d.AnnualRevenue)
            ));
        }

        // Calculate Monthly Revenue trends from real database deals
        var monthlyRevenueList = allDeals
            .GroupBy(d => d.CreatedAt.ToString("MMM yyyy"))
            .Select(g => new MonthlyRevenueDataDto(
                g.Key,
                g.Sum(d => d.AnnualRevenue),
                g.Count()
            ))
            .ToList();

        // Calculate Conversion and Target Progress Stats
        double conversionRate = totalLeads > 0 ? Math.Round(((double)wonDealsCount / totalLeads) * 100, 1) : 0;
        decimal targetAmount = 100000m;
        double targetProgress = targetAmount > 0 ? (double)Math.Min(100, Math.Round((totalRevenueGenerated / targetAmount) * 100, 1)) : 0;

        var conversionStats = new ConversionStatsDto(
            conversionRate,
            targetAmount,
            totalRevenueGenerated,
            targetProgress
        );

        return new DashboardStatsDto(
            totalLeads,
            avgTimeToCloseDays,
            ongoingDealsCount,
            wonDealsCount,
            totalRevenueGenerated,
            lostDealsByReason,
            perEmployeeMetrics,
            monthlyRevenueList,
            conversionStats
        );
    }
}
