namespace FusionOS.Modules.Manufacturing.Application.Machines.Contracts;

public interface IMachineRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Domain.Machines.Machine?> GetByIdAsync(Guid companyId, Guid machineId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Machines.Machine machine, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Machines.Machine>> ListByWorkCenterIdAsync(Guid companyId, Guid workCenterId, CancellationToken cancellationToken = default);
}
