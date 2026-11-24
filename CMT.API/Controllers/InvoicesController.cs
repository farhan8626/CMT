// ============================================================
//  CMT.API – InvoicesController
//  POST   /api/invoices                    → sp_CreateInvoiceWithStockUpdate
//  GET    /api/invoices?customerId=&paymentStatus=&page=&pageSize=
//  GET    /api/invoices/{id}
//  PATCH  /api/invoices/{id}/payment
//  GET    /api/invoices/{id}/pdf            → returns PDF binary
//  POST   /api/invoices/{id}/email          → sends PDF via MailKit
// ============================================================
using CMT.API.Extensions;
using CMT.API.Models;
using CMT.Core.DTOs.Invoice;
using CMT.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMT.API.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
[Produces("application/json")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoices;
    public InvoicesController(IInvoiceService invoices) => _invoices = invoices;

    /// <summary>
    /// Create a new invoice (Non-GST).
    /// Executes sp_CreateInvoiceWithStockUpdate atomically:
    ///   – Inserts Invoice + InvoiceItems
    ///   – Deducts stock for every Product line item
    ///   – Logs a StockLog 'Sale' entry per product
    ///   – If TicketId is provided, marks the ticket as 'Billed'
    /// Returns the new InvoiceId and InvoiceNumber.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateInvoiceResult>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 409)]  // insufficient stock
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var userId  = User.GetUserId();
        var payload = request with { CreatedByUserId = userId };

        var result = await _invoices.CreateAsync(payload);
        return CreatedAtAction(nameof(GetById), new { id = result.NewInvoiceId },
            ApiResponse<CreateInvoiceResult>.Ok(result, $"Invoice {result.NewInvoiceNumber} created successfully."));
    }

    /// <summary>
    /// List invoices with optional filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvoiceSummaryDto>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int?    customerId    = null,
        [FromQuery] string? paymentStatus = null,
        [FromQuery] int     page          = 1,
        [FromQuery] int     pageSize      = 20)
    {
        var invoices = await _invoices.GetAllAsync(customerId, paymentStatus, pageSize, page);
        return Ok(ApiResponse<IEnumerable<InvoiceSummaryDto>>.Ok(invoices));
    }

    /// <summary>
    /// Get a full invoice including customer details, ticket reference, and all line items.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _invoices.GetByIdAsync(id);
        return Ok(ApiResponse<InvoiceDto>.Ok(invoice));
    }

    /// <summary>Update payment status and method for an existing invoice.</summary>
    [HttpPatch("{id:int}/payment")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        await _invoices.UpdatePaymentAsync(id, request);
        return Ok(ApiResponse.Ok("Payment status updated successfully."));
    }

    /// <summary>
    /// Generate and download the PDF for an invoice.
    /// Returns application/pdf – CMT non-GST invoice (SubTotal, Discount, GrandTotal in ₹).
    /// </summary>
    [HttpGet("{id:int}/pdf")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        // Need invoice number for the filename
        var invoice  = await _invoices.GetByIdAsync(id);
        var pdfBytes = await _invoices.GeneratePdfAsync(id);

        return File(
            pdfBytes,
            "application/pdf",
            $"{invoice.InvoiceNumber}.pdf");
    }

    /// <summary>
    /// Email the invoice PDF to the customer.
    /// Requires the customer to have an email address on file.
    /// </summary>
    [HttpPost("{id:int}/email")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> SendEmail(int id)
    {
        await _invoices.SendEmailAsync(id);
        return Ok(ApiResponse.Ok("Invoice emailed to customer successfully."));
    }
}
