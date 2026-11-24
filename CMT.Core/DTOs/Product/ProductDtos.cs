// ============================================================
//  CMT.Core – DTOs: Product (Inventory)
// ============================================================
using System.ComponentModel.DataAnnotations;

namespace CMT.Core.DTOs.Product;

public record CreateProductRequest(
    [Required, MaxLength(50)]  string   SKU,
    [Required, MaxLength(200)] string   Name,
    [Required, MaxLength(100)] string   Category,
    [MaxLength(100)]           string?  Brand,
    [Range(0, double.MaxValue)] decimal PurchasePrice,
    [Range(0, double.MaxValue)] decimal SellingPrice,
    [Range(0, int.MaxValue)]    int     StockQuantity,
    [Range(0, int.MaxValue)]    int     LowStockThreshold
);

public record UpdateProductRequest(
    [Required, MaxLength(50)]  string   SKU,
    [Required, MaxLength(200)] string   Name,
    [Required, MaxLength(100)] string   Category,
    [MaxLength(100)]           string?  Brand,
    [Range(0, double.MaxValue)] decimal PurchasePrice,
    [Range(0, double.MaxValue)] decimal SellingPrice,
    [Range(0, int.MaxValue)]    int     LowStockThreshold
);

public record StockInRequest(
    [Range(1, int.MaxValue)] int    Quantity,
    [MaxLength(500)]         string? Notes
);

public record ProductDto(
    int      Id,
    string   SKU,
    string   Name,
    string   Category,
    string?  Brand,
    decimal  PurchasePrice,
    decimal  SellingPrice,
    int      StockQuantity,
    int      LowStockThreshold,
    bool     IsLowStock,
    DateTime UpdatedAt
);
