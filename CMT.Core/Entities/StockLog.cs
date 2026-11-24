// ============================================================
//  CMT.Core – Entity: StockLog
// ============================================================
namespace CMT.Core.Entities;

public class StockLog
{
    public int      Id              { get; set; }
    public int      ProductId       { get; set; }
    public string   ChangeType      { get; set; } = null!;  // StockIn | Sale | Adjustment
    public int      QuantityChanged { get; set; }
    public int?     ReferenceId     { get; set; }           // InvoiceId for Sales
    public string?  Notes           { get; set; }
    public DateTime CreatedAt       { get; set; }

    // Nav
    public Product Product { get; set; } = null!;
}
