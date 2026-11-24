// ============================================================
//  CMT.API – RepairServicesController
//  GET    /api/repair-services?activeOnly=true
//  GET    /api/repair-services/{id}
//  POST   /api/repair-services        [Admin]
//  PUT    /api/repair-services/{id}   [Admin]
// ============================================================
using CMT.API.Models;
using CMT.Core.DTOs.RepairService;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/repair-services")]
[Authorize]
[Produces("application/json")]
public class RepairServicesController : ControllerBase
{
    private readonly IRepairServiceService _services;
    public RepairServicesController(IRepairServiceService services) => _services = services;

    /// <summary>List repair services. Set activeOnly=false to include inactive ones.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RepairServiceDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true)
    {
        var services = await _services.GetAllAsync(activeOnly);
        return Ok(ApiResponse<IEnumerable<RepairServiceDto>>.Ok(services));
    }

    /// <summary>Get a single repair service by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RepairServiceDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _services.GetByIdAsync(id);
        return Ok(ApiResponse<RepairServiceDto>.Ok(service));
    }

    /// <summary>Create a new repair service. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RepairServiceDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateRepairServiceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var service = await _services.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = service.Id },
            ApiResponse<RepairServiceDto>.Ok(service, "Repair service created successfully."));
    }

    /// <summary>Update a repair service (name, charge, active status). Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RepairServiceDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRepairServiceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var service = await _services.UpdateAsync(id, request);
        return Ok(ApiResponse<RepairServiceDto>.Ok(service, "Repair service updated successfully."));
    }
}
