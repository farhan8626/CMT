// ============================================================
//  CMT.Infrastructure – Placeholder Email Service (MailKit)
//  Full implementation in Phase 3.
// ============================================================
using CMT.Core.DTOs.Invoice;
using CMT.Infrastructure.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace CMT.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) => _config = config;

    public async Task SendInvoiceAsync(InvoiceDto invoice, byte[] pdfBytes)
    {
        var smtp    = _config.GetSection("SmtpSettings");
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            smtp["FromName"] ?? "CMT Electrical Shop",
            smtp["FromEmail"]!));
        message.To.Add(new MailboxAddress(invoice.CustomerName, invoice.CustomerEmail));
        message.Subject = $"Your Invoice {invoice.InvoiceNumber} from CMT Electrical Shop";

        var body    = new TextPart("plain")
        {
            Text = $"Dear {invoice.CustomerName},\n\n" +
                   $"Please find attached your invoice {invoice.InvoiceNumber} " +
                   $"dated {invoice.InvoiceDate:dd-MMM-yyyy}.\n\n" +
                   $"Amount Due: ₹{invoice.GrandTotal:N2}\n\n" +
                   $"Thank you for choosing CMT Electrical Shop!\n\nRegards,\nCMT Team"
        };

        var attachment = new MimePart("application", "pdf")
        {
            Content            = new MimeContent(new MemoryStream(pdfBytes)),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName           = $"{invoice.InvoiceNumber}.pdf"
        };

        message.Body = new Multipart("mixed") { body, attachment };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            smtp["Host"]!,
            int.Parse(smtp["Port"] ?? "587"),
            bool.Parse(smtp["UseStartTls"] ?? "true")
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.SslOnConnect);

        await client.AuthenticateAsync(smtp["Username"]!, smtp["Password"]!);
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
    }
}
