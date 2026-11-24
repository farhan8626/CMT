// ============================================================
//  CMT.Infrastructure – RepairService Service
// ============================================================
using CMT.Core.DTOs.RepairService;
using CMT.Core.Entities;
using CMT.Core.Interfaces;

namespace CMT.Infrastructure.Services;

public class RepairServiceService : IRepairServiceService
{
    private readonly IRepairServiceRepository _repo;
    public RepairServiceService(IRepairServiceRepository repo) => _repo = repo;

    public async Task<IEnumerable<RepairServiceDto>> GetAllAsync(bool activeOnly = true)
    {
        var services = await _repo.GetAllAsync(activeOnly);
        return services.Select(Map);
    }

    public async Task<RepairServiceDto> GetByIdAsync(int id)
    {
        var svc = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Repair service {id} not found.");
        return Map(svc);
    }

    public async Task<RepairServiceDto> CreateAsync(CreateRepairServiceRequest request)
    {
        var svc = new RepairService
        {
            ServiceName = request.ServiceName.Trim(),
            Description = request.Description?.Trim(),
            BaseCharge  = request.BaseCharge,
            IsActive    = true
        };
        await _repo.CreateAsync(svc);
        return Map(svc);
    }

    public async Task<RepairServiceDto> UpdateAsync(int id, UpdateRepairServiceRequest request)
    {
        var svc = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Repair service {id} not found.");

        svc.ServiceName = request.ServiceName.Trim();
        svc.Description = request.Description?.Trim();
        svc.BaseCharge  = request.BaseCharge;
        svc.IsActive    = request.IsActive;

        await _repo.UpdateAsync(svc);
        return Map(svc);
    }

    private static RepairServiceDto Map(RepairService s) =>
        new(s.Id, s.ServiceName, s.Description, s.BaseCharge, s.IsActive);
}
