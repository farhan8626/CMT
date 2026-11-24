// ============================================================
//  CMT.Core – DTOs: Invoice  (Non-GST)
// ============================================================
using System.ComponentModel.DataAnnotations;

namespace CMT.Core.DTOs.Invoice;

// --- Line item used in create request (mirrors the TVP) ---
public record InvoiceItemRequest(
    [Required]                 string   ItemType,     // Product | Service
    int?                               ProductId,
    int?                               ServiceId,
    [Required, MaxLength(300)] string   ItemName,
    [Range(0.001, double.MaxValue)] decimal Quantity,
    [Range(0, double.MaxValue)] decimal  UnitPrice
);

// --- Create invoice request ---
public record CreateInvoiceRequest(
    [Required]                   int      CustomerId,
    int?                                  TicketId,
    [Required]                   DateOnly InvoiceDate,
    [Required]                   decimal  SubTotal,
    decimal                               Discount,
    [Required]                   decimal  GrandTotal,
    [Required]                   string   PaymentStatus,  // Paid | Pending | Partial
    string?                               PaymentMethod,  // Cash | UPI | Card
    [MaxLength(2000)]            string?  Notes,
    int                                   CreatedByUserId,
    [Required, MinLength(1)]     List<InvoiceItemRequest> Items
);

// --- Update payment status only ---
public record UpdatePaymentRequest(
    [Required] string  PaymentStatus,
    string?            PaymentMethod
);

// --- Read DTOs ---
public record InvoiceItemDto(
    int      Id,
    string   ItemType,
    int?     ProductId,
    int?     ServiceId,
    string   ItemName,
    decimal  Quantity,
    decimal  UnitPrice,
    decimal  LineTotal
);

public record InvoiceDto(
    int      Id,
    string   InvoiceNumber,
    int      CustomerId,
    string   CustomerName,
    string   CustomerPhone,
    string?  CustomerEmail,
    string?  CustomerAddress,
    int?     TicketId,
    string?  TicketNumber,
    DateOnly InvoiceDate,
    decimal  SubTotal,
    decimal  Discount,
    decimal  GrandTotal,
    string   PaymentStatus,
    string?  PaymentMethod,
    string?  Notes,
    int      CreatedByUserId,
    string   CreatedByName,
    DateTime CreatedAt,
    List<InvoiceItemDto> Items
);

public record InvoiceSummaryDto(
    int      Id,
    string   InvoiceNumber,
    DateOnly InvoiceDate,
    string   CustomerName,
    decimal  GrandTotal,
    string   PaymentStatus,
    string?  PaymentMethod,
    int?     TicketId,
    string?  TicketNumber
);

public record CreateInvoiceResult(
    int    NewInvoiceId,
    string NewInvoiceNumber
);
