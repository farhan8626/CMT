// ============================================================
//  CMT.Infrastructure – Dashboard Repository (Dapper)
//  Calls sp_GetDashboardSummary via QueryMultipleAsync
// ============================================================
using CMT.Core.DTOs.Dashboard;
using CMT.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CMT.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly string _connectionString;

    public DashboardRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // sp_GetDashboardSummary returns 4 result sets
        using var multi = await conn.QueryMultipleAsync(
            "EXEC dbo.sp_GetDashboardSummary",
            commandType: System.Data.CommandType.Text);

        // Result Set 1 – single KPI row
        var kpiRow = await multi.ReadSingleAsync();
        var kpi = new DashboardKpiDto(
            TodaysSales:         (decimal)kpiRow.TodaysSales,
            TodaysInvoiceCount:  (int)kpiRow.TodaysInvoiceCount,
            MonthlyRevenue:      (decimal)kpiRow.MonthlyRevenue,
            MonthlyInvoiceCount: (int)kpiRow.MonthlyInvoiceCount,
            ActiveRepairTickets: (int)kpiRow.ActiveRepairTickets,
            ReadyForPickup:      (int)kpiRow.ReadyForPickup,
            OverdueTickets:      (int)kpiRow.OverdueTickets,
            LowStockItemCount:   (int)kpiRow.LowStockItemCount,
            TotalCustomers:      (int)kpiRow.TotalCustomers,
            TotalProducts:       (int)kpiRow.TotalProducts
        );

        // Result Set 2 – recent invoices
        var recentRaw = await multi.ReadAsync();
        var recentInvoices = recentRaw.Select(r => new RecentInvoiceDto(
            Id:            (int)r.Id,
            InvoiceNumber: (string)r.InvoiceNumber,
            InvoiceDate:   DateOnly.FromDateTime((DateTime)r.InvoiceDate),
            CustomerName:  (string)r.CustomerName,
            CustomerPhone: (string)r.CustomerPhone,
            GrandTotal:    (decimal)r.GrandTotal,
            PaymentStatus: (string)r.PaymentStatus,
            PaymentMethod: r.PaymentMethod is DBNull ? null : (string?)r.PaymentMethod,
            TicketId:      r.TicketId is DBNull ? null : (int?)r.TicketId,
            TicketNumber:  r.TicketNumber is DBNull ? null : (string?)r.TicketNumber
        )).ToList();

        // Result Set 3 – low stock products
        var lowStockRaw = await multi.ReadAsync();
        var lowStockProducts = lowStockRaw.Select(r => new LowStockProductDto(
            Id:                (int)r.Id,
            SKU:               (string)r.SKU,
            Name:              (string)r.Name,
            Category:          (string)r.Category,
            Brand:             r.Brand is DBNull ? null : (string?)r.Brand,
            StockQuantity:     (int)r.StockQuantity,
            LowStockThreshold: (int)r.LowStockThreshold,
            SellingPrice:      (decimal)r.SellingPrice,
            ShortfallQty:      (int)r.ShortfallQty
        )).ToList();

        // Result Set 4 – urgent/overdue tickets
        var urgentRaw = await multi.ReadAsync();
        var urgentTickets = urgentRaw.Select(r => new UrgentTicketDto(
            Id:                  (int)r.Id,
            TicketNumber:        (string)r.TicketNumber,
            DeviceName:          (string)r.DeviceName,
            Status:              (string)r.Status,
            ExpectedDeliveryDate: r.ExpectedDeliveryDate is DBNull
                                   ? null
                                   : DateOnly.FromDateTime((DateTime)r.ExpectedDeliveryDate),
            ReceivedDate:        DateOnly.FromDateTime((DateTime)r.ReceivedDate),
            CustomerName:        (string)r.CustomerName,
            CustomerPhone:       (string)r.CustomerPhone,
            DaysOverdue:         (int)r.DaysOverdue
        )).ToList();

        return new DashboardSummaryDto(kpi, recentInvoices, lowStockProducts, urgentTickets);
    }
}
