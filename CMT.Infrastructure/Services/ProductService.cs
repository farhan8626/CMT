// ============================================================
//  CMT.Infrastructure – Product Service
// ============================================================
using CMT.Core.DTOs.Product;
using CMT.Core.Entities;
using CMT.Core.Interfaces;

namespace CMT.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<IEnumerable<ProductDto>> GetAllAsync(string? search = null, string? category = null)
    {
        var products = await _repo.GetAllAsync(search, category);
        return products.Select(Map);
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");
        return Map(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        if (await _repo.SkuExistsAsync(request.SKU))
            throw new ArgumentException($"SKU '{request.SKU}' already exists.");

        var product = new Product
        {
            SKU               = request.SKU.Trim().ToUpper(),
            Name              = request.Name.Trim(),
            Category          = request.Category.Trim(),
            Brand             = request.Brand?.Trim(),
            PurchasePrice     = request.PurchasePrice,
            SellingPrice      = request.SellingPrice,
            StockQuantity     = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            UpdatedAt         = DateTime.UtcNow
        };
        await _repo.CreateAsync(product);
        return Map(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        if (await _repo.SkuExistsAsync(request.SKU, excludeId: id))
            throw new ArgumentException($"SKU '{request.SKU}' already exists.");

        product.SKU               = request.SKU.Trim().ToUpper();
        product.Name              = request.Name.Trim();
        product.Category          = request.Category.Trim();
        product.Brand             = request.Brand?.Trim();
        product.PurchasePrice     = request.PurchasePrice;
        product.SellingPrice      = request.SellingPrice;
        product.LowStockThreshold = request.LowStockThreshold;
        product.UpdatedAt         = DateTime.UtcNow;

        await _repo.UpdateAsync(product);
        return Map(product);
    }

    public async Task StockInAsync(int id, StockInRequest request)
    {
        var product = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");
        await _repo.StockInAsync(id, request.Quantity, request.Notes);
    }

    public Task<IEnumerable<string>> GetCategoriesAsync() =>
        _repo.GetCategoriesAsync();

    private static ProductDto Map(Product p) =>
        new(p.Id, p.SKU, p.Name, p.Category, p.Brand,
            p.PurchasePrice, p.SellingPrice,
            p.StockQuantity, p.LowStockThreshold,
            p.IsLowStock, p.UpdatedAt);
}
