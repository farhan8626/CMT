// ============================================================
//  CMT.Infrastructure – Invoice Service
//  CreateAsync delegates to Dapper SP; reads use EF Core
// ============================================================
using CMT.Core.DTOs.Invoice;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Repositories;
using System.Linq;

namespace CMT.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository  _repo;
    private readonly IEmailService       _email;
    private readonly IPdfService         _pdf;

    public InvoiceService(
        IInvoiceRepository repo,
        IEmailService      email,
        IPdfService        pdf)
    {
        _repo  = repo;
        _email = email;
        _pdf   = pdf;
    }

    public async Task<CreateInvoiceResult> CreateAsync(CreateInvoiceRequest request)
    {
        // Validation
        var expectedTotal = Math.Round(request.SubTotal - request.Discount, 2);
        var actualTotal   = Math.Round(request.GrandTotal, 2);
        if (actualTotal != expectedTotal)
            throw new ArgumentException(
                $"GrandTotal (₹{request.GrandTotal:N2}) must equal SubTotal minus Discount (₹{expectedTotal:N2}).");

        if (!request.Items.Any())
            throw new ArgumentException("Invoice must have at least one line item.");

        // Delegate to Dapper SP (atomic transaction in DB)
        return await ((InvoiceRepository)_repo).CreateWithStockUpdateAsync(request);
    }

    public async Task<InvoiceDto> GetByIdAsync(int id)
    {
        var invoice = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        return new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerId,
            invoice.Customer.FullName,
            invoice.Customer.Phone,
            invoice.Customer.Email,
            invoice.Customer.Address,
            invoice.TicketId,
            invoice.Ticket?.TicketNumber,
            invoice.InvoiceDate,
            invoice.SubTotal,
            invoice.Discount,
            invoice.GrandTotal,
            invoice.PaymentStatus,
            invoice.PaymentMethod,
            invoice.Notes,
            invoice.CreatedByUserId,
            invoice.CreatedByUser.FullName,
            invoice.CreatedAt,
            invoice.Items.Select(i => new InvoiceItemDto(
                i.Id, i.ItemType,
                i.ProductId, i.ServiceId,
                i.ItemName, i.Quantity,
                i.UnitPrice, i.LineTotal
            )).ToList()
        );
    }

    public async Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync(
        int? customerId = null, string? paymentStatus = null,
        int pageSize = 20, int page = 1)
    {
        var invoices = await _repo.GetAllAsync(customerId, paymentStatus, pageSize, page);
        return invoices.Select(i => new InvoiceSummaryDto(
            i.Id, i.InvoiceNumber, i.InvoiceDate,
            i.Customer.FullName, i.GrandTotal,
            i.PaymentStatus, i.PaymentMethod,
            i.TicketId, i.Ticket?.TicketNumber));
    }

    public async Task UpdatePaymentAsync(int id, UpdatePaymentRequest request)
    {
        var invoice = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        await _repo.UpdatePaymentAsync(id, request.PaymentStatus, request.PaymentMethod);
    }

    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var invoiceDto = await GetByIdAsync(id);
        return await _pdf.GenerateInvoicePdfAsync(invoiceDto);
    }

    public async Task SendEmailAsync(int id)
    {
        var invoiceDto = await GetByIdAsync(id);
        if (string.IsNullOrWhiteSpace(invoiceDto.CustomerEmail))
            throw new InvalidOperationException("Customer does not have an email address on file.");

        var pdfBytes = await _pdf.GenerateInvoicePdfAsync(invoiceDto);
        await _email.SendInvoiceAsync(invoiceDto, pdfBytes);
    }
}

// ---- Interface stubs (implemented in Phase 3) ----
public interface IEmailService
{
    Task SendInvoiceAsync(InvoiceDto invoice, byte[] pdfBytes);
}

public interface IPdfService
{
    Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice);
}
