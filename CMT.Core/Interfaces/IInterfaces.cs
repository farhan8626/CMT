// ============================================================
//  CMT.Core – Interface Contracts (Repositories + Services)
//  All interfaces in one file per domain for brevity.
// ============================================================
using CMT.Core.DTOs.Auth;
using CMT.Core.DTOs.Customer;
using CMT.Core.DTOs.Dashboard;
using CMT.Core.DTOs.Invoice;
using CMT.Core.DTOs.Product;
using CMT.Core.DTOs.RepairTicket;
using CMT.Core.DTOs.RepairService;
using CMT.Core.Entities;

namespace CMT.Core.Interfaces;

// ---- Auth / Users ----
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}

public interface IAuthService
{
    Task<LoginResponse>  LoginAsync(LoginRequest request);
    Task<UserDto>        CreateUserAsync(CreateUserRequest request);
    Task<UserDto>        UpdateUserAsync(int id, UpdateUserRequest request);
    Task                 ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto>        GetUserByIdAsync(int id);
}

// ---- Customers ----
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(int id);
    Task<IEnumerable<Customer>> GetAllAsync(string? search = null);
    Task<Customer> CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
    Task<CustomerDetailDto> GetDetailAsync(int id);
}

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync(string? search = null);
    Task<CustomerDetailDto>        GetByIdAsync(int id);
    Task<CustomerDto>              CreateAsync(CreateCustomerRequest request);
    Task<CustomerDto>              UpdateAsync(int id, UpdateCustomerRequest request);
}

// ---- Products ----
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(string? search = null, string? category = null);
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
    Task StockInAsync(int productId, int quantity, string? notes = null);
    Task<IEnumerable<string>> GetCategoriesAsync();
}

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(string? search = null, string? category = null);
    Task<ProductDto>              GetByIdAsync(int id);
    Task<ProductDto>              CreateAsync(CreateProductRequest request);
    Task<ProductDto>              UpdateAsync(int id, UpdateProductRequest request);
    Task                          StockInAsync(int id, StockInRequest request);
    Task<IEnumerable<string>>     GetCategoriesAsync();
}

// ---- Repair Services ----
public interface IRepairServiceRepository
{
    Task<RepairService?> GetByIdAsync(int id);
    Task<IEnumerable<RepairService>> GetAllAsync(bool activeOnly = true);
    Task<RepairService> CreateAsync(RepairService service);
    Task UpdateAsync(RepairService service);
}

public interface IRepairServiceService
{
    Task<IEnumerable<RepairServiceDto>> GetAllAsync(bool activeOnly = true);
    Task<RepairServiceDto>              GetByIdAsync(int id);
    Task<RepairServiceDto>              CreateAsync(CreateRepairServiceRequest request);
    Task<RepairServiceDto>              UpdateAsync(int id, UpdateRepairServiceRequest request);
}

// ---- Repair Tickets ----
public interface IRepairTicketRepository
{
    Task<RepairTicket?> GetByIdAsync(int id);
    Task<IEnumerable<RepairTicket>> GetAllAsync(string? status = null, int? customerId = null);
    Task<RepairTicket> CreateAsync(RepairTicket ticket);
    Task UpdateAsync(RepairTicket ticket);
    Task<string> GenerateTicketNumberAsync();
}

public interface IRepairTicketService
{
    Task<IEnumerable<TicketSummaryDto>> GetAllAsync(string? status = null, int? customerId = null);
    Task<TicketDto>                     GetByIdAsync(int id);
    Task<TicketDto>                     CreateAsync(CreateTicketRequest request);
    Task<TicketDto>                     UpdateAsync(int id, UpdateTicketRequest request);
    Task<TicketDto>                     UpdateStatusAsync(int id, string status);
}

// ---- Invoices ----
public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id);
    Task<IEnumerable<Invoice>> GetAllAsync(int? customerId = null, string? paymentStatus = null, int pageSize = 20, int page = 1);
    Task UpdatePaymentAsync(int id, string paymentStatus, string? paymentMethod);
    // Note: CreateWithStockUpdateAsync lives on the concrete InvoiceRepository (Dapper + SP)
    // and is called directly by InvoiceService via a cast.
}

public interface IInvoiceService
{
    Task<CreateInvoiceResult>          CreateAsync(CreateInvoiceRequest request);
    Task<InvoiceDto>                   GetByIdAsync(int id);
    Task<IEnumerable<InvoiceSummaryDto>> GetAllAsync(int? customerId = null, string? paymentStatus = null, int pageSize = 20, int page = 1);
    Task                               UpdatePaymentAsync(int id, UpdatePaymentRequest request);
    Task<byte[]>                       GeneratePdfAsync(int id);
    Task                               SendEmailAsync(int id);
}

// ---- Dashboard (Dapper) ----
public interface IDashboardRepository
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}
