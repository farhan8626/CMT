// ============================================================
//  CMT.API – DashboardController
//  GET /api/dashboard
//  Calls sp_GetDashboardSummary via Dapper QueryMultipleAsync
//  Returns 4 result sets: KPIs, Recent Invoices, Low Stock, Urgent Tickets
// ============================================================
using CMT.API.Models;
using CMT.Core.DTOs.Dashboard;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;
    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    /// <summary>
    /// Returns a full dashboard summary from sp_GetDashboardSummary:
    ///   • KPI row  — Today's Sales (₹), Monthly Revenue (₹), Active/Ready/Overdue tickets, Low Stock count
    ///   • Last 10 invoices
    ///   • Top 10 lowest-stock products
    ///   • Top 10 overdue repair tickets
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), 200)]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboard.GetSummaryAsync();
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
    }
}
