// ============================================================
//  CMT.Infrastructure – Dashboard Service
// ============================================================
using CMT.Core.DTOs.Dashboard;
using CMT.Core.Interfaces;

namespace CMT.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;
    public DashboardService(IDashboardRepository repo) => _repo = repo;

    public Task<DashboardSummaryDto> GetSummaryAsync() =>
        _repo.GetSummaryAsync();
}
