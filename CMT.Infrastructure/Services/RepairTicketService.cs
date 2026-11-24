// ============================================================
//  CMT.Infrastructure – RepairTicket Service
// ============================================================
using CMT.Core.DTOs.RepairTicket;
using CMT.Core.Entities;
using CMT.Core.Interfaces;

namespace CMT.Infrastructure.Services;

public class RepairTicketService : IRepairTicketService
{
    private static readonly string[] ValidStatuses =
    [
        "Received", "In Repair", "Waiting for Parts", "Ready for Pickup", "Billed", "Cancelled"
    ];

    private readonly IRepairTicketRepository _repo;
    public RepairTicketService(IRepairTicketRepository repo) => _repo = repo;

    public async Task<IEnumerable<TicketSummaryDto>> GetAllAsync(string? status = null, int? customerId = null)
    {
        var tickets = await _repo.GetAllAsync(status, customerId);
        return tickets.Select(MapSummary);
    }

    public async Task<TicketDto> GetByIdAsync(int id)
    {
        var ticket = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Repair ticket {id} not found.");
        return MapDetail(ticket);
    }

    public async Task<TicketDto> CreateAsync(CreateTicketRequest request)
    {
        var ticketNumber = await _repo.GenerateTicketNumberAsync();

        var ticket = new RepairTicket
        {
            TicketNumber         = ticketNumber,
            CustomerId           = request.CustomerId,
            DeviceName           = request.DeviceName.Trim(),
            ReportedIssue        = request.ReportedIssue.Trim(),
            EstimatedCost        = request.EstimatedCost,
            Status               = "Received",
            ReceivedDate         = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            Notes                = request.Notes?.Trim(),
            CreatedByUserId      = request.CreatedByUserId,
            CreatedAt            = DateTime.UtcNow,
            UpdatedAt            = DateTime.UtcNow
        };

        await _repo.CreateAsync(ticket);
        return await GetByIdAsync(ticket.Id);
    }

    public async Task<TicketDto> UpdateAsync(int id, UpdateTicketRequest request)
    {
        var ticket = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Repair ticket {id} not found.");

        ticket.DeviceName           = request.DeviceName.Trim();
        ticket.ReportedIssue        = request.ReportedIssue.Trim();
        ticket.EstimatedCost        = request.EstimatedCost;
        ticket.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        ticket.Notes                = request.Notes?.Trim();

        await _repo.UpdateAsync(ticket);
        return await GetByIdAsync(id);
    }

    public async Task<TicketDto> UpdateStatusAsync(int id, string status)
    {
        if (!ValidStatuses.Contains(status))
            throw new ArgumentException($"Invalid status '{status}'. Must be one of: {string.Join(", ", ValidStatuses)}");

        var ticket = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Repair ticket {id} not found.");

        // Don't allow manually setting Billed – that's done via invoice creation
        if (status == "Billed")
            throw new ArgumentException("Status 'Billed' is set automatically when an invoice is created.");

        ticket.Status = status;
        await _repo.UpdateAsync(ticket);
        return await GetByIdAsync(id);
    }

    private static TicketSummaryDto MapSummary(RepairTicket t) =>
        new(t.Id, t.TicketNumber,
            t.Customer?.FullName ?? "",
            t.Customer?.Phone    ?? "",
            t.DeviceName, t.Status,
            t.ReceivedDate, t.ExpectedDeliveryDate,
            t.EstimatedCost);

    private static TicketDto MapDetail(RepairTicket t) =>
        new(t.Id, t.TicketNumber,
            t.CustomerId,
            t.Customer?.FullName  ?? "",
            t.Customer?.Phone     ?? "",
            t.DeviceName,
            t.ReportedIssue,
            t.EstimatedCost,
            t.Status,
            t.ReceivedDate,
            t.ExpectedDeliveryDate,
            t.CompletedDate,
            t.InvoiceId,
            t.Invoice?.InvoiceNumber,
            t.Notes,
            t.CreatedByUserId,
            t.CreatedByUser?.FullName ?? "",
            t.CreatedAt, t.UpdatedAt);
}
