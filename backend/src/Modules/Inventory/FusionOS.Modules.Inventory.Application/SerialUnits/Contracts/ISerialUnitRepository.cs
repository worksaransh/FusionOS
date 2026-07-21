using FusionOS.Modules.Inventory.Domain.SerialUnits;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;

public interface ISerialUnitRepository
{
    Task<SerialUnit?> GetByIdAsync(Guid companyId, Guid serialUnitId, CancellationToken cancellationToken = default);

    /// <summary>The "scan a serial and find it" lookup — exact match across the whole company, not scoped to a single product.</summary>
    Task<SerialUnit?> GetBySerialNumberAsync(Guid companyId, string serialNumber, CancellationToken cancellationToken = default);

    Task<bool> SerialNumberExistsAsync(Guid companyId, Guid productId, string serialNumber, CancellationToken cancellationToken = default);
    Task AddAsync(SerialUnit unit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SerialUnit>> ListByProductAsync(Guid companyId, Guid productId, SerialUnitStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByProductAsync(Guid companyId, Guid productId, SerialUnitStatus? status, CancellationToken cancellationToken = default);
}
