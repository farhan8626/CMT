// ============================================================
//  CMT.Infrastructure – Customer Repository (EF Core)
// ============================================================
using CMT.Core.DTOs.Customer;
using CMT.Core.Entities;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CMT.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CmtDbContext _db;
    public CustomerRepository(CmtDbContext db) => _db = db;

    public async Task<Customer?> GetByIdAsync(int id) =>
        await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Customer>> GetAllAsync(string? search = null)
    {
        var q = _db.Customers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.FullName.Contains(search) || c.Phone.Contains(search));
        return await q.OrderBy(c => c.FullName).ToListAsync();
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
    {
        var q = _db.Customers.Where(c => c.Phone == phone);
        if (excludeId.HasValue) q = q.Where(c => c.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task<CustomerDetailDto> GetDetailAsync(int id)
    {
        var customer = await _db.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerDetailDto(
                c.Id,
                c.FullName,
                c.Phone,
                c.Email,
                c.Address,
                c.CreatedAt,
                c.Invoices.Count,
                c.Invoices.Sum(i => i.GrandTotal),
                c.RepairTickets.Count
            ))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return customer
            ?? throw new KeyNotFoundException($"Customer {id} not found.");
    }
}
