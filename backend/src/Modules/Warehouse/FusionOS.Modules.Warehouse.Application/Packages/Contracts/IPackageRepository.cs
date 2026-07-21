namespace FusionOS.Modules.Warehouse.Application.Packages.Contracts;

public interface IPackageRepository
{
    Task<Domain.Packages.Package?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Packages.Package package, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Packages.Package>> ListByPickListAsync(Guid companyId, Guid pickListId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByPickListAsync(Guid companyId, Guid pickListId, CancellationToken cancellationToken = default);
    Task<bool> PackageNumberExistsAsync(Guid companyId, Guid pickListId, string packageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums the quantity already packaged for one product across every existing Package recorded
    /// against this pick list — the cross-aggregate quantity guard CreatePackageCommandHandler uses
    /// to ensure a PickList's total packed quantity per product never exceeds what was actually
    /// picked for that product (PickListLine.QuantityPicked), mirroring
    /// IDispatchRepository.GetDispatchedQuantityAsync / IInvoiceRepository.GetInvoicedQuantityAsync
    /// (Sales) exactly.
    /// </summary>
    Task<decimal> GetPackagedQuantityAsync(Guid companyId, Guid pickListId, Guid productId, CancellationToken cancellationToken = default);
}
