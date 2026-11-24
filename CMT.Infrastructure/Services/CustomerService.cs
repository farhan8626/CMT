// ============================================================
//  CMT.Infrastructure – Customer Service
// ============================================================
using CMT.Core.DTOs.Customer;
using CMT.Core.Entities;
using CMT.Core.Interfaces;

namespace CMT.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;
    public CustomerService(ICustomerRepository repo) => _repo = repo;

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(string? search = null)
    {
        var customers = await _repo.GetAllAsync(search);
        return customers.Select(Map);
    }

    public async Task<CustomerDetailDto> GetByIdAsync(int id) =>
        await _repo.GetDetailAsync(id);

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request)
    {
        if (await _repo.PhoneExistsAsync(request.Phone))
            throw new ArgumentException($"Phone number '{request.Phone}' is already registered.");

        var customer = new Customer
        {
            FullName  = request.FullName.Trim(),
            Phone     = request.Phone.Trim(),
            Email     = request.Email?.Trim().ToLower(),
            Address   = request.Address?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        await _repo.CreateAsync(customer);
        return Map(customer);
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request)
    {
        var customer = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Customer {id} not found.");

        if (await _repo.PhoneExistsAsync(request.Phone, excludeId: id))
            throw new ArgumentException($"Phone number '{request.Phone}' is already registered.");

        customer.FullName = request.FullName.Trim();
        customer.Phone    = request.Phone.Trim();
        customer.Email    = request.Email?.Trim().ToLower();
        customer.Address  = request.Address?.Trim();

        await _repo.UpdateAsync(customer);
        return Map(customer);
    }

    private static CustomerDto Map(Customer c) =>
        new(c.Id, c.FullName, c.Phone, c.Email, c.Address, c.CreatedAt);
}
