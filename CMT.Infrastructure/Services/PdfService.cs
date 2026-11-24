// ============================================================
//  CMT.Infrastructure – PDF Service (QuestPDF)
//  Non-GST invoice: SubTotal, Discount, GrandTotal only.
//  Branded for CMT Electrical Shop.
//  Full design in Phase 3; this is the working foundation.
// ============================================================
using CMT.Core.DTOs.Invoice;
using CMT.Infrastructure.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CMT.Infrastructure.Services;

public class PdfService : IPdfService
{
    public Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(10));

                page.Header().Element(ComposeHeader(invoice));
                page.Content().Element(ComposeContent(invoice));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("CMT Electrical Shop  •  ");
                    x.Span("Non-GST Invoice  •  Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return Task.FromResult(doc.GeneratePdf());
    }

    // ---- Header ----
    private static Action<IContainer> ComposeHeader(InvoiceDto inv) => c =>
    {
        c.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("CMT").FontSize(22).Bold().FontColor("#1e40af");
                col.Item().Text("Electrical Spare Parts & Repair Shop").FontSize(11);
                col.Item().Text("Non-GST Invoice").FontSize(9).FontColor("#6b7280");
            });

            row.ConstantItem(200).Column(col =>
            {
                col.Item().AlignRight().Text($"Invoice #: {inv.InvoiceNumber}").Bold();
                col.Item().AlignRight().Text($"Date: {inv.InvoiceDate:dd-MMM-yyyy}");
                col.Item().AlignRight()
                   .Text($"Status: {inv.PaymentStatus}")
                   .FontColor(inv.PaymentStatus == "Paid" ? "#16a34a" : "#dc2626");
                if (inv.TicketNumber is not null)
                    col.Item().AlignRight().Text($"Ticket: {inv.TicketNumber}").FontColor("#6b7280");
            });
        });

        c.PaddingTop(8).LineHorizontal(1).LineColor("#e5e7eb");
    };

    // ---- Content ----
    private static Action<IContainer> ComposeContent(InvoiceDto inv) => c =>
    {
        c.Column(col =>
        {
            // Customer block
            col.Item().PaddingTop(10).Column(cust =>
            {
                cust.Item().Text("Bill To:").Bold().FontSize(10);
                cust.Item().Text(inv.CustomerName).Bold();
                cust.Item().Text($"Phone: {inv.CustomerPhone}");
                if (inv.CustomerEmail is not null)
                    cust.Item().Text($"Email: {inv.CustomerEmail}");
                if (inv.CustomerAddress is not null)
                    cust.Item().Text(inv.CustomerAddress);
            });

            // Items table
            col.Item().PaddingTop(16).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(30);    // #
                    cols.RelativeColumn(4);     // Description
                    cols.RelativeColumn(1.2f);  // Type
                    cols.ConstantColumn(60);    // Qty
                    cols.ConstantColumn(80);    // Unit Price
                    cols.ConstantColumn(90);    // Total
                });

                // Header row
                table.Header(h =>
                {
                    static IContainer HeaderCell(IContainer cell) =>
                        cell.DefaultTextStyle(t => t.Bold()).Background("#1e40af")
                            .PaddingVertical(5).PaddingHorizontal(6);

                    static void HeaderText(IContainer cell, string text, bool right = false)
                    {
                        var c = HeaderCell(cell);
                        if (right) c.AlignRight().Text(text).FontColor("#ffffff");
                        else       c.Text(text).FontColor("#ffffff");
                    }

                    HeaderText(h.Cell(), "#");
                    HeaderText(h.Cell(), "Description");
                    HeaderText(h.Cell(), "Type");
                    HeaderText(h.Cell(), "Qty",      right: true);
                    HeaderText(h.Cell(), "Rate (₹)", right: true);
                    HeaderText(h.Cell(), "Total (₹)", right: true);
                });

                // Data rows
                for (int i = 0; i < inv.Items.Count; i++)
                {
                    var item   = inv.Items[i];
                    var bg     = i % 2 == 0 ? "#f9fafb" : "#ffffff";

                    static IContainer DataCell(IContainer c, string bg) =>
                        c.Background(bg).PaddingVertical(4).PaddingHorizontal(6);

                    DataCell(table.Cell(), bg).Text($"{i + 1}");
                    DataCell(table.Cell(), bg).Text(item.ItemName);
                    DataCell(table.Cell(), bg).Text(item.ItemType).FontColor("#6b7280");
                    DataCell(table.Cell(), bg).AlignRight().Text($"{item.Quantity:0.###}");
                    DataCell(table.Cell(), bg).AlignRight().Text($"₹{item.UnitPrice:N2}");
                    DataCell(table.Cell(), bg).AlignRight().Text($"₹{item.LineTotal:N2}");
                }
            });

            // Totals block
            col.Item().PaddingTop(10).AlignRight().Column(totals =>
            {
                totals.Item().Row(r =>
                {
                    r.RelativeItem().AlignRight().Text("Sub Total:").Bold();
                    r.ConstantItem(100).AlignRight().Text($"₹{inv.SubTotal:N2}");
                });

                if (inv.Discount > 0)
                {
                    totals.Item().Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Discount:").FontColor("#dc2626");
                        r.ConstantItem(100).AlignRight().Text($"- ₹{inv.Discount:N2}").FontColor("#dc2626");
                    });
                }

                totals.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().AlignRight().Text("GRAND TOTAL:").Bold().FontSize(13);
                    r.ConstantItem(100).AlignRight().Text($"₹{inv.GrandTotal:N2}").Bold().FontSize(13).FontColor("#1e40af");
                });

                if (inv.PaymentMethod is not null)
                {
                    totals.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().AlignRight().Text("Payment Method:").FontColor("#6b7280");
                        r.ConstantItem(100).AlignRight().Text(inv.PaymentMethod).FontColor("#6b7280");
                    });
                }
            });

            // Notes
            if (inv.Notes is not null)
            {
                col.Item().PaddingTop(16).Column(n =>
                {
                    n.Item().Text("Notes:").Bold();
                    n.Item().Text(inv.Notes).FontColor("#6b7280");
                });
            }

            col.Item().PaddingTop(20).AlignCenter()
               .Text("Thank you for your business! — CMT Electrical Shop")
               .FontColor("#6b7280").Italic();
        });
    };
}
