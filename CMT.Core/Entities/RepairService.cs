// ============================================================
//  CMT.Core – Entity: RepairService
// ============================================================
namespace CMT.Core.Entities;

public class RepairService
{
    public int     Id          { get; set; }
    public string  ServiceName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal BaseCharge  { get; set; }
    public bool    IsActive    { get; set; } = true;

    // Nav
    public ICollection<InvoiceItem> InvoiceItems { get; set; } = [];
}
