// ============================================================
//  CMT.Core – Entity: Customer
// ============================================================
namespace CMT.Core.Entities;

public class Customer
{
    public int     Id        { get; set; }
    public string  FullName  { get; set; } = null!;
    public string  Phone     { get; set; } = null!;
    public string? Email     { get; set; }
    public string? Address   { get; set; }
    public DateTime CreatedAt{ get; set; }

    // Nav
    public ICollection<RepairTicket> RepairTickets { get; set; } = [];
    public ICollection<Invoice>      Invoices      { get; set; } = [];
}
