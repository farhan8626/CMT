// ============================================================
//  CMT.Core – Entity: RepairTicket
// ============================================================
namespace CMT.Core.Entities;

public class RepairTicket
{
    public int      Id                   { get; set; }
    public string   TicketNumber         { get; set; } = null!;   // CMT-TKT-XXXX
    public int      CustomerId           { get; set; }
    public string   DeviceName           { get; set; } = null!;
    public string   ReportedIssue        { get; set; } = null!;
    public decimal? EstimatedCost        { get; set; }
    public string   Status               { get; set; } = "Received";
    public DateOnly ReceivedDate         { get; set; }
    public DateOnly? ExpectedDeliveryDate{ get; set; }
    public DateOnly? CompletedDate       { get; set; }
    public int?     InvoiceId            { get; set; }
    public string?  Notes                { get; set; }
    public int      CreatedByUserId      { get; set; }
    public DateTime CreatedAt            { get; set; }
    public DateTime UpdatedAt            { get; set; }

    // Nav
    public Customer     Customer      { get; set; } = null!;
    public User         CreatedByUser { get; set; } = null!;
    public Invoice?     Invoice       { get; set; }
}
