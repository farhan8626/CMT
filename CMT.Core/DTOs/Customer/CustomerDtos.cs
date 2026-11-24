// ============================================================
//  CMT.Core – DTOs: Customer
// ============================================================
using System.ComponentModel.DataAnnotations;

namespace CMT.Core.DTOs.Customer;

public record CreateCustomerRequest(
    [Required, MaxLength(150)] string  FullName,
    [Required, MaxLength(20)]  string  Phone,
    [MaxLength(200)]           string? Email,
    [MaxLength(500)]           string? Address
);

public record UpdateCustomerRequest(
    [Required, MaxLength(150)] string  FullName,
    [Required, MaxLength(20)]  string  Phone,
    [MaxLength(200)]           string? Email,
    [MaxLength(500)]           string? Address
);

public record CustomerDto(
    int      Id,
    string   FullName,
    string   Phone,
    string?  Email,
    string?  Address,
    DateTime CreatedAt
);

public record CustomerDetailDto(
    int      Id,
    string   FullName,
    string   Phone,
    string?  Email,
    string?  Address,
    DateTime CreatedAt,
    int      TotalInvoices,
    decimal  TotalSpent,
    int      TotalTickets
);
