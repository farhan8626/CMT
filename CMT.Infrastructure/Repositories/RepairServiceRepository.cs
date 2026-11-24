// ============================================================
//  CMT.Infrastructure – RepairService Repository (EF Core)
// ============================================================
using CMT.Core.Entities;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CMT.Infrastructure.Repositories;

public class RepairServiceRepository : IRepairServiceRepository
{
    private readonly CmtDbContext _db;
    public RepairServiceRepository(CmtDbContext db) => _db = db;

    public async Task<RepairService?> GetByIdAsync(int id) =>
        await _db.RepairServices.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public async Task<IEnumerable<RepairService>> GetAllAsync(bool activeOnly = true)
    {
        var q = _db.RepairServices.AsNoTracking().AsQueryable();
        if (activeOnly) q = q.Where(s => s.IsActive);
        return await q.OrderBy(s => s.ServiceName).ToListAsync();
    }

    public async Task<RepairService> CreateAsync(RepairService service)
    {
        _db.RepairServices.Add(service);
        await _db.SaveChangesAsync();
        return service;
    }

    public async Task UpdateAsync(RepairService service)
    {
        _db.RepairServices.Update(service);
        await _db.SaveChangesAsync();
    }
}
