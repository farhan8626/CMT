// ============================================================
//  CMT.Core – DTOs: RepairTicket
// ============================================================
using System.ComponentModel.DataAnnotations;

namespace CMT.Core.DTOs.RepairTicket;

public record CreateTicketRequest(
    [Required]                 int      CustomerId,
    [Required, MaxLength(200)] string   DeviceName,
    [Required, MaxLength(1000)]string   ReportedIssue,
    decimal?                            EstimatedCost,
    DateOnly?                           ExpectedDeliveryDate,
    [MaxLength(2000)]          string?  Notes,
    int                                 CreatedByUserId
);

public record UpdateTicketRequest(
    [Required, MaxLength(200)] string   DeviceName,
    [Required, MaxLength(1000)]string   ReportedIssue,
    decimal?                            EstimatedCost,
    DateOnly?                           ExpectedDeliveryDate,
    [MaxLength(2000)]          string?  Notes
);

public record UpdateTicketStatusRequest(
    [Required] string Status
);

public record TicketDto(
    int       Id,
    string    TicketNumber,
    int       CustomerId,
    string    CustomerName,
    string    CustomerPhone,
    string    DeviceName,
    string    ReportedIssue,
    decimal?  EstimatedCost,
    string    Status,
    DateOnly  ReceivedDate,
    DateOnly? ExpectedDeliveryDate,
    DateOnly? CompletedDate,
    int?      InvoiceId,
    string?   InvoiceNumber,
    string?   Notes,
    int       CreatedByUserId,
    string    CreatedByName,
    DateTime  CreatedAt,
    DateTime  UpdatedAt
);

public record TicketSummaryDto(
    int       Id,
    string    TicketNumber,
    string    CustomerName,
    string    CustomerPhone,
    string    DeviceName,
    string    Status,
    DateOnly  ReceivedDate,
    DateOnly? ExpectedDeliveryDate,
    decimal?  EstimatedCost
);
