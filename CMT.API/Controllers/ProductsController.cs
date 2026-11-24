// ============================================================
//  CMT.API – ProductsController (Inventory)
//  GET    /api/products?search=&category=
//  GET    /api/products/{id}
//  POST   /api/products               [Admin]
//  PUT    /api/products/{id}          [Admin]
//  DELETE /api/products/{id}          [Admin]
//  POST   /api/products/{id}/stock-in
//  GET    /api/products/categories
// ============================================================
using CMT.API.Extensions;
using CMT.API.Models;
using CMT.Core.DTOs.Product;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;
    public ProductsController(IProductService products) => _products = products;

    /// <summary>
    /// List all products. Filter by search (name/SKU) or category.
    /// Response includes IsLowStock flag for badge rendering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search   = null,
        [FromQuery] string? category = null)
    {
        var products = await _products.GetAllAsync(search, category);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
    }

    /// <summary>Get a single product by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _products.GetByIdAsync(id);
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>Get distinct product categories for filter dropdown.</summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), 200)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _products.GetCategoriesAsync();
        return Ok(ApiResponse<IEnumerable<string>>.Ok(categories));
    }

    /// <summary>Create a new product. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var product = await _products.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            ApiResponse<ProductDto>.Ok(product, "Product created successfully."));
    }

    /// <summary>Update product details (not stock). Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var product = await _products.UpdateAsync(id, request);
        return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated successfully."));
    }

    /// <summary>
    /// Add stock for a product ("Stock In" / restock).
    /// Logs a StockIn entry in StockLogs.
    /// Available to both Admin and Staff.
    /// </summary>
    [HttpPost("{id:int}/stock-in")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> StockIn(int id, [FromBody] StockInRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        await _products.StockInAsync(id, request);
        return Ok(ApiResponse.Ok($"Added {request.Quantity} unit(s) to stock successfully."));
    }
}
