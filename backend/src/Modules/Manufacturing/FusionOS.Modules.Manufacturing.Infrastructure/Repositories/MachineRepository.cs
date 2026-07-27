using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;
using FusionOS.Modules.Manufacturing.Domain.Machines;
using FusionOS.Modules.Manufacturing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Repositories;

public sealed class MachineRepository : IMachineRepository
{
    private readonly ManufacturingDbContext _context;

    public MachineRepository(ManufacturingDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.Machines.AnyAsync(m => m.CompanyId == companyId && m.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public Task<Machine?> GetByIdAsync(Guid companyId, Guid machineId, CancellationToken cancellationToken = default) =>
        _context.Machines.FirstOrDefaultAsync(m => m.CompanyId == companyId && m.Id == machineId, cancellationToken);

    public async Task AddAsync(Machine machine, CancellationToken cancellationToken = default) =>
        await _context.Machines.AddAsync(machine, cancellationToken);

    public async Task<IReadOnlyList<Machine>> ListByWorkCenterIdAsync(Guid companyId, Guid workCenterId, CancellationToken cancellationToken = default) =>
        await _context.Machines
            .Where(m => m.CompanyId == companyId && m.WorkCenterId == workCenterId)
            .OrderBy(m => m.Code)
            .ToListAsync(cancellationToken);
}
