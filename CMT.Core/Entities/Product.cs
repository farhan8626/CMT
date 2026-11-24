// ============================================================
//  CMT.Core – Entity: Product (Spare Parts Inventory)
// ============================================================
namespace CMT.Core.Entities;

public class Product
{
    public int      Id                { get; set; }
    public string   SKU               { get; set; } = null!;
    public string   Name              { get; set; } = null!;
    public string   Category          { get; set; } = null!;
    public string?  Brand             { get; set; }
    public decimal  PurchasePrice     { get; set; }
    public decimal  SellingPrice      { get; set; }
    public int      StockQuantity     { get; set; }
    public int      LowStockThreshold { get; set; } = 5;
    public DateTime UpdatedAt         { get; set; }

    // Computed (not mapped)
    public bool IsLowStock => StockQuantity <= LowStockThreshold;

    // Nav
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = [];
    public ICollection<StockLog>    StockLogs    { get; set; } = [];
}
