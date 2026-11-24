// ============================================================
//  CMT.Core – Entity: Invoice  (Non-GST – no tax columns)
// ============================================================
namespace CMT.Core.Entities;

public class Invoice
{
    public int      Id              { get; set; }
    public string   InvoiceNumber   { get; set; } = null!;   // CMT-INV-XXXX
    public int      CustomerId      { get; set; }
    public int?     TicketId        { get; set; }
    public DateOnly InvoiceDate     { get; set; }
    public decimal  SubTotal        { get; set; }
    public decimal  Discount        { get; set; }
    public decimal  GrandTotal      { get; set; }
    public string   PaymentStatus   { get; set; } = "Pending";  // Paid | Pending | Partial
    public string?  PaymentMethod   { get; set; }               // Cash | UPI | Card
    public string?  Notes           { get; set; }
    public int      CreatedByUserId { get; set; }
    public DateTime CreatedAt       { get; set; }

    // Nav
    public Customer              Customer      { get; set; } = null!;
    public RepairTicket?         Ticket        { get; set; }
    public User                  CreatedByUser { get; set; } = null!;
    public ICollection<InvoiceItem> Items      { get; set; } = [];
}
