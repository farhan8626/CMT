// ============================================================
//  CMT.Core – Entity: InvoiceItem
// ============================================================
namespace CMT.Core.Entities;

public class InvoiceItem
{
    public int      Id        { get; set; }
    public int      InvoiceId { get; set; }
    public string   ItemType  { get; set; } = null!;   // Product | Service
    public int?     ProductId { get; set; }
    public int?     ServiceId { get; set; }
    public string   ItemName  { get; set; } = null!;
    public decimal  Quantity  { get; set; }
    public decimal  UnitPrice { get; set; }
    public decimal  LineTotal { get; set; }             // persisted computed in DB

    // Nav
    public Invoice       Invoice  { get; set; } = null!;
    public Product?      Product  { get; set; }
    public RepairService? Service { get; set; }
}
