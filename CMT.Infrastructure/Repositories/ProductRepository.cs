// ============================================================
//  CMT.Infrastructure – Product Repository (EF Core)
// ============================================================
using CMT.Core.Entities;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CMT.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CmtDbContext _db;
    public ProductRepository(CmtDbContext db) => _db = db;

    public async Task<Product?> GetByIdAsync(int id) =>
        await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Product>> GetAllAsync(string? search = null, string? category = null)
    {
        var q = _db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(p => p.Category == category);
        return await q.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
    {
        var q = _db.Products.Where(p => p.SKU == sku);
        if (excludeId.HasValue) q = q.Where(p => p.Id != excludeId.Value);
        return await q.AnyAsync();
    }

    public async Task StockInAsync(int productId, int quantity, string? notes = null)
    {
        var product = await _db.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException($"Product {productId} not found.");

        product.StockQuantity += quantity;
        product.UpdatedAt      = DateTime.UtcNow;

        _db.StockLogs.Add(new StockLog
        {
            ProductId       = productId,
            ChangeType      = "StockIn",
            QuantityChanged = quantity,
            Notes           = notes ?? $"Stock added: +{quantity} units"
        });

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync() =>
        await _db.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
}
