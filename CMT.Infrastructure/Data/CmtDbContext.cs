// ============================================================
//  CMT.Infrastructure – EF Core DbContext
// ============================================================
using CMT.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CMT.Infrastructure.Data;

public class CmtDbContext : DbContext
{
    public CmtDbContext(DbContextOptions<CmtDbContext> options) : base(options) { }

    public DbSet<User>          Users          => Set<User>();
    public DbSet<Customer>      Customers      => Set<Customer>();
    public DbSet<Product>       Products       => Set<Product>();
    public DbSet<RepairService> RepairServices => Set<RepairService>();
    public DbSet<RepairTicket>  RepairTickets  => Set<RepairTicket>();
    public DbSet<Invoice>       Invoices       => Set<Invoice>();
    public DbSet<InvoiceItem>   InvoiceItems   => Set<InvoiceItem>();
    public DbSet<StockLog>      StockLogs      => Set<StockLog>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ---- Users ----
        mb.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("Staff");
            e.Property(x => x.IsActive).HasDefaultValue(true);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ---- Customers ----
        mb.Entity<Customer>(e =>
        {
            e.ToTable("Customers");
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Phone).IsUnique();
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Address).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // ---- Products ----
        mb.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasKey(x => x.Id);
            e.Property(x => x.SKU).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.SKU).IsUnique();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100).IsRequired();
            e.Property(x => x.Brand).HasMaxLength(100);
            e.Property(x => x.PurchasePrice).HasColumnType("decimal(12,2)");
            e.Property(x => x.SellingPrice).HasColumnType("decimal(12,2)");
            e.Property(x => x.LowStockThreshold).HasDefaultValue(5);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Ignore(x => x.IsLowStock);  // computed in app, not in DB column
        });

        // ---- RepairServices ----
        mb.Entity<RepairService>(e =>
        {
            e.ToTable("RepairServices");
            e.HasKey(x => x.Id);
            e.Property(x => x.ServiceName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.BaseCharge).HasColumnType("decimal(12,2)");
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        // ---- RepairTickets ----
        mb.Entity<RepairTicket>(e =>
        {
            e.ToTable("RepairTickets");
            e.HasKey(x => x.Id);
            e.Property(x => x.TicketNumber).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.TicketNumber).IsUnique();
            e.Property(x => x.DeviceName).HasMaxLength(200).IsRequired();
            e.Property(x => x.ReportedIssue).HasMaxLength(1000).IsRequired();
            e.Property(x => x.EstimatedCost).HasColumnType("decimal(12,2)");
            e.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Received");
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(x => x.Customer)
             .WithMany(c => c.RepairTickets)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.CreatedByUser)
             .WithMany(u => u.CreatedTickets)
             .HasForeignKey(x => x.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            // InvoiceId is set by the SP – EF reads it back, no cascade
            e.HasOne(x => x.Invoice)
             .WithOne()
             .HasForeignKey<RepairTicket>(x => x.InvoiceId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);
        });

        // ---- Invoices ----
        mb.Entity<Invoice>(e =>
        {
            e.ToTable("Invoices");
            e.HasKey(x => x.Id);
            e.Property(x => x.InvoiceNumber).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.Property(x => x.SubTotal).HasColumnType("decimal(12,2)");
            e.Property(x => x.Discount).HasColumnType("decimal(12,2)").HasDefaultValue(0m);
            e.Property(x => x.GrandTotal).HasColumnType("decimal(12,2)");
            e.Property(x => x.PaymentStatus).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(x => x.PaymentMethod).HasMaxLength(20);
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(x => x.Customer)
             .WithMany(c => c.Invoices)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Ticket)
             .WithMany()
             .HasForeignKey(x => x.TicketId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);

            e.HasOne(x => x.CreatedByUser)
             .WithMany(u => u.CreatedInvoices)
             .HasForeignKey(x => x.CreatedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- InvoiceItems ----
        mb.Entity<InvoiceItem>(e =>
        {
            e.ToTable("InvoiceItems");
            e.HasKey(x => x.Id);
            e.Property(x => x.ItemType).HasMaxLength(20).IsRequired();
            e.Property(x => x.ItemName).HasMaxLength(300).IsRequired();
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(12,2)");
            // LineTotal is a persisted computed column in SQL Server – map as read-only
            e.Property(x => x.LineTotal).HasColumnType("decimal(12,2)")
             .ValueGeneratedOnAddOrUpdate();

            e.HasOne(x => x.Invoice)
             .WithMany(i => i.Items)
             .HasForeignKey(x => x.InvoiceId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Product)
             .WithMany(p => p.InvoiceItems)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);

            e.HasOne(x => x.Service)
             .WithMany(s => s.InvoiceItems)
             .HasForeignKey(x => x.ServiceId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);
        });

        // ---- StockLogs ----
        mb.Entity<StockLog>(e =>
        {
            e.ToTable("StockLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.ChangeType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Notes).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            e.HasOne(x => x.Product)
             .WithMany(p => p.StockLogs)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
