// ============================================================
//  CMT.Core – DTOs: Dashboard
// ============================================================
namespace CMT.Core.DTOs.Dashboard;

public record DashboardKpiDto(
    decimal TodaysSales,
    int     TodaysInvoiceCount,
    decimal MonthlyRevenue,
    int     MonthlyInvoiceCount,
    int     ActiveRepairTickets,
    int     ReadyForPickup,
    int     OverdueTickets,
    int     LowStockItemCount,
    int     TotalCustomers,
    int     TotalProducts
);

public record RecentInvoiceDto(
    int      Id,
    string   InvoiceNumber,
    DateOnly InvoiceDate,
    string   CustomerName,
    string   CustomerPhone,
    decimal  GrandTotal,
    string   PaymentStatus,
    string?  PaymentMethod,
    int?     TicketId,
    string?  TicketNumber
);

public record LowStockProductDto(
    int     Id,
    string  SKU,
    string  Name,
    string  Category,
    string? Brand,
    int     StockQuantity,
    int     LowStockThreshold,
    decimal SellingPrice,
    int     ShortfallQty
);

public record UrgentTicketDto(
    int       Id,
    string    TicketNumber,
    string    DeviceName,
    string    Status,
    DateOnly? ExpectedDeliveryDate,
    DateOnly  ReceivedDate,
    string    CustomerName,
    string    CustomerPhone,
    int       DaysOverdue
);

public record DashboardSummaryDto(
    DashboardKpiDto          Kpi,
    List<RecentInvoiceDto>   RecentInvoices,
    List<LowStockProductDto> LowStockProducts,
    List<UrgentTicketDto>    UrgentTickets
);
