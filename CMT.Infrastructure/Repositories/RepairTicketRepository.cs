// ============================================================
//  CMT.Infrastructure – RepairTicket Repository (EF Core)
//  Calls sp_GenerateTicketNumber via Dapper for atomic number gen
// ============================================================
using CMT.Core.Entities;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CMT.Infrastructure.Repositories;

public class RepairTicketRepository : IRepairTicketRepository
{
    private readonly CmtDbContext _db;
    private readonly string       _connectionString;

    public RepairTicketRepository(CmtDbContext db, IConfiguration config)
    {
        _db               = db;
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    public async Task<RepairTicket?> GetByIdAsync(int id) =>
        await _db.RepairTickets
            .Include(t => t.Customer)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Invoice)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<RepairTicket>> GetAllAsync(string? status = null, int? customerId = null)
    {
        var q = _db.RepairTickets
            .Include(t => t.Customer)
            .Include(t => t.CreatedByUser)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(t => t.Status == status);

        if (customerId.HasValue)
            q = q.Where(t => t.CustomerId == customerId.Value);

        return await q.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<RepairTicket> CreateAsync(RepairTicket ticket)
    {
        _db.RepairTickets.Add(ticket);
        await _db.SaveChangesAsync();
        return ticket;
    }

    public async Task UpdateAsync(RepairTicket ticket)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        _db.RepairTickets.Update(ticket);
        await _db.SaveChangesAsync();
    }

    // Calls sp_GenerateTicketNumber via Dapper – sequence-based, race-safe
    public async Task<string> GenerateTicketNumberAsync()
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        var result = await conn.QuerySingleAsync<string>(
            "EXEC dbo.sp_GenerateTicketNumber",
            commandType: System.Data.CommandType.Text);
        return result;
    }
}
