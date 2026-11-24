// ============================================================
//  CMT.Core – Entity: User
// ============================================================
namespace CMT.Core.Entities;

public class User
{
    public int    Id           { get; set; }
    public string FullName     { get; set; } = null!;
    public string Email        { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role         { get; set; } = "Staff";   // Admin | Staff
    public bool   IsActive     { get; set; } = true;
    public DateTime CreatedAt  { get; set; }

    // Nav
    public ICollection<RepairTicket> CreatedTickets  { get; set; } = [];
    public ICollection<Invoice>      CreatedInvoices { get; set; } = [];
}
