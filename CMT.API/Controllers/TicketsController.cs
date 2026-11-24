// ============================================================
//  CMT.API – TicketsController (Repair Job Cards)
//  GET    /api/tickets?status=&customerId=
//  GET    /api/tickets/{id}
//  POST   /api/tickets
//  PUT    /api/tickets/{id}
//  PATCH  /api/tickets/{id}/status
// ============================================================
using CMT.API.Extensions;
using CMT.API.Models;
using CMT.Core.DTOs.RepairTicket;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
[Produces("application/json")]
public class TicketsController : ControllerBase
{
    private readonly IRepairTicketService _tickets;
    public TicketsController(IRepairTicketService tickets) => _tickets = tickets;

    /// <summary>
    /// List repair tickets. Filter by status or customer.
    /// Valid statuses: Received | In Repair | Waiting for Parts | Ready for Pickup | Billed | Cancelled
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TicketSummaryDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status     = null,
        [FromQuery] int?    customerId = null)
    {
        var tickets = await _tickets.GetAllAsync(status, customerId);
        return Ok(ApiResponse<IEnumerable<TicketSummaryDto>>.Ok(tickets));
    }

    /// <summary>Get a single ticket with full detail (customer, user, invoice link).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _tickets.GetByIdAsync(id);
        return Ok(ApiResponse<TicketDto>.Ok(ticket));
    }

    /// <summary>
    /// Create a new repair ticket (Job Card).
    /// Ticket number is auto-generated as CMT-TKT-XXXX via SQL SEQUENCE.
    /// Status defaults to "Received".
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        // Inject the authenticated user's ID as the creator
        var userId  = User.GetUserId();
        var payload = request with { CreatedByUserId = userId };

        var ticket = await _tickets.CreateAsync(payload);
        return CreatedAtAction(nameof(GetById), new { id = ticket.Id },
            ApiResponse<TicketDto>.Ok(ticket, $"Ticket {ticket.TicketNumber} created successfully."));
    }

    /// <summary>Update ticket details (device, issue, estimated cost, expected date, notes).</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var ticket = await _tickets.UpdateAsync(id, request);
        return Ok(ApiResponse<TicketDto>.Ok(ticket, "Ticket updated successfully."));
    }

    /// <summary>
    /// Update a ticket's workflow status.
    /// Valid transitions: Received → In Repair → Waiting for Parts → Ready for Pickup → Cancelled.
    /// "Billed" is set automatically by the invoice creation stored procedure.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(ApiResponse<TicketDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var ticket = await _tickets.UpdateStatusAsync(id, request.Status);
        return Ok(ApiResponse<TicketDto>.Ok(ticket, $"Ticket status updated to '{request.Status}'."));
    }
}
