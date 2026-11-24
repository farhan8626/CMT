// ============================================================
//  CMT.API – CustomersController
//  GET    /api/customers?search=
//  GET    /api/customers/{id}
//  POST   /api/customers
//  PUT    /api/customers/{id}
//  GET    /api/customers/{id}/invoices     → invoice history
//  GET    /api/customers/{id}/tickets      → ticket history
// ============================================================
using CMT.API.Models;
using CMT.Core.DTOs.Customer;
using CMT.Core.DTOs.Invoice;
using CMT.Core.DTOs.RepairTicket;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;
    private readonly IInvoiceService  _invoices;
    private readonly IRepairTicketService _tickets;

    public CustomersController(
        ICustomerService      customers,
        IInvoiceService       invoices,
        IRepairTicketService  tickets)
    {
        _customers = customers;
        _invoices  = invoices;
        _tickets   = tickets;
    }

    /// <summary>List all customers. Optional full-text search by name or phone.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CustomerDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        var customers = await _customers.GetAllAsync(search);
        return Ok(ApiResponse<IEnumerable<CustomerDto>>.Ok(customers));
    }

    /// <summary>Get a customer with totals (invoice count, spend, ticket count).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _customers.GetByIdAsync(id);
        return Ok(ApiResponse<CustomerDetailDto>.Ok(customer));
    }

    /// <summary>Create a new customer.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var customer = await _customers.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id },
            ApiResponse<CustomerDto>.Ok(customer, "Customer created successfully."));
    }

    /// <summary>Update a customer's details.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var customer = await _customers.UpdateAsync(id, request);
        return Ok(ApiResponse<CustomerDto>.Ok(customer, "Customer updated successfully."));
    }

    /// <summary>Get paginated invoice history for a customer.</summary>
    [HttpGet("{id:int}/invoices")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvoiceSummaryDto>>), 200)]
    public async Task<IActionResult> GetInvoices(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var invoices = await _invoices.GetAllAsync(customerId: id, pageSize: pageSize, page: page);
        return Ok(ApiResponse<IEnumerable<InvoiceSummaryDto>>.Ok(invoices));
    }

    /// <summary>Get repair ticket history for a customer.</summary>
    [HttpGet("{id:int}/tickets")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TicketSummaryDto>>), 200)]
    public async Task<IActionResult> GetTickets(int id)
    {
        var tickets = await _tickets.GetAllAsync(customerId: id);
        return Ok(ApiResponse<IEnumerable<TicketSummaryDto>>.Ok(tickets));
    }
}
