// ============================================================
//  CMT.Core – DTOs: RepairService
// ============================================================
using System.ComponentModel.DataAnnotations;

namespace CMT.Core.DTOs.RepairService;

public record CreateRepairServiceRequest(
    [Required, MaxLength(200)] string   ServiceName,
    [MaxLength(500)]           string?  Description,
    [Range(0, double.MaxValue)] decimal BaseCharge
);

public record UpdateRepairServiceRequest(
    [Required, MaxLength(200)] string   ServiceName,
    [MaxLength(500)]           string?  Description,
    [Range(0, double.MaxValue)] decimal BaseCharge,
    bool IsActive
);

public record RepairServiceDto(
    int     Id,
    string  ServiceName,
    string? Description,
    decimal BaseCharge,
    bool    IsActive
);
