// ============================================================
//  CMT.Infrastructure – Invoice Repository
//  Uses Dapper + AsTableValuedParameter for sp_CreateInvoiceWithStockUpdate
//  Uses EF Core for all other CRUD
// ============================================================
using System.Data;
using CMT.Core.DTOs.Invoice;
using CMT.Core.Entities;
using CMT.Core.Interfaces;
using CMT.Infrastructure.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CMT.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly CmtDbContext _db;
    private readonly string       _connectionString;

    public InvoiceRepository(CmtDbContext db, IConfiguration config)
    {
        _db               = db;
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // ----------------------------------------------------------------
    // CREATE – delegates to Dapper + TVP stored procedure
    // Returns the new InvoiceId and InvoiceNumber
    // ----------------------------------------------------------------
    public async Task<CreateInvoiceResult> CreateWithStockUpdateAsync(CreateInvoiceRequest request)
    {
        // Build the Table-Valued Parameter DataTable
        // (matches dbo.InvoiceItemsTableType column layout)
        var itemsTable = new DataTable();
        itemsTable.Columns.Add("ItemType",   typeof(string));
        itemsTable.Columns.Add("ProductId",  typeof(int));
        itemsTable.Columns.Add("ServiceId",  typeof(int));
        itemsTable.Columns.Add("ItemName",   typeof(string));
        itemsTable.Columns.Add("Quantity",   typeof(decimal));
        itemsTable.Columns.Add("UnitPrice",  typeof(decimal));

        foreach (var item in request.Items)
        {
            itemsTable.Rows.Add(
                item.ItemType,
                item.ProductId.HasValue ? (object)item.ProductId.Value : DBNull.Value,
                item.ServiceId.HasValue ? (object)item.ServiceId.Value : DBNull.Value,
                item.ItemName,
                item.Quantity,
                item.UnitPrice
            );
        }

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var parameters = new DynamicParameters();
        parameters.Add("@CustomerId",       request.CustomerId);
        parameters.Add("@TicketId",         request.TicketId,       DbType.Int32,   ParameterDirection.Input);
        parameters.Add("@InvoiceDate",      request.InvoiceDate.ToDateTime(TimeOnly.MinValue), DbType.Date);
        parameters.Add("@SubTotal",         request.SubTotal,        DbType.Decimal, ParameterDirection.Input, precision: 12, scale: 2);
        parameters.Add("@Discount",         request.Discount,        DbType.Decimal, ParameterDirection.Input, precision: 12, scale: 2);
        parameters.Add("@GrandTotal",       request.GrandTotal,      DbType.Decimal, ParameterDirection.Input, precision: 12, scale: 2);
        parameters.Add("@PaymentStatus",    request.PaymentStatus);
        parameters.Add("@PaymentMethod",    request.PaymentMethod);
        parameters.Add("@Notes",            request.Notes);
        parameters.Add("@CreatedByUserId",  request.CreatedByUserId);
        parameters.Add("@NewInvoiceId",     dbType: DbType.Int32,   direction: ParameterDirection.Output);
        parameters.Add("@NewInvoiceNumber", dbType: DbType.String,  direction: ParameterDirection.Output, size: 30);

        // AsTableValuedParameter – passes the DataTable as the TVP
        parameters.Add("@Items", itemsTable.AsTableValuedParameter("dbo.InvoiceItemsTableType"));

        // Execute the stored procedure (it also returns a SELECT result set)
        await conn.QueryAsync(
            "dbo.sp_CreateInvoiceWithStockUpdate",
            parameters,
            commandType: CommandType.StoredProcedure);

        var newId     = parameters.Get<int>("@NewInvoiceId");
        var newNumber = parameters.Get<string>("@NewInvoiceNumber");

        return new CreateInvoiceResult(newId, newNumber);
    }

    // ----------------------------------------------------------------
    // READ – EF Core
    // ----------------------------------------------------------------
    public async Task<Invoice?> GetByIdAsync(int id) =>
        await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Ticket)
            .Include(i => i.CreatedByUser)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Product)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<IEnumerable<Invoice>> GetAllAsync(
        int? customerId = null,
        string? paymentStatus = null,
        int pageSize = 20,
        int page = 1)
    {
        var query = _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Ticket)
            .AsNoTracking()
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(i => i.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(paymentStatus))
            query = query.Where(i => i.PaymentStatus == paymentStatus);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task UpdatePaymentAsync(int id, string paymentStatus, string? paymentMethod)
    {
        await _db.Invoices
            .Where(i => i.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(i => i.PaymentStatus, paymentStatus)
                .SetProperty(i => i.PaymentMethod, paymentMethod));
    }
}
