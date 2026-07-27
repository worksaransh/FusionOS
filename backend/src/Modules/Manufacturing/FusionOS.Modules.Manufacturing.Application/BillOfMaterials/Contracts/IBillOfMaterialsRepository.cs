namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

public interface IBillOfMaterialsRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Domain.BillOfMaterials.BillOfMaterials?> GetByIdAsync(Guid companyId, Guid billOfMaterialsId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.BillOfMaterials.BillOfMaterials billOfMaterials, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.BillOfMaterials.BillOfMaterials>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>Every active, current-version BillOfMaterials for a product (Alternative BOM, Phase 6) — the primary one plus any alternatives.</summary>
    Task<IReadOnlyList<Domain.BillOfMaterials.BillOfMaterials>> ListActiveByProductIdAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>The active, current-version, primary BillOfMaterials for a product, or null if none — the default a Work Order or MRP explosion picks when no explicit BillOfMaterialsId is given.</summary>
    Task<Domain.BillOfMaterials.BillOfMaterials?> GetPrimaryByProductIdAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// BOM Versioning (Phase 6, 2026-07-22). Every BillOfMaterials in the same version chain as
    /// <paramref name="billOfMaterialsId"/> (including inactive/non-current ones), ordered by
    /// Version descending. Version chains share the same Code, so this resolves the chain via a
    /// single Code+CompanyId query rather than walking PreviousVersionId one hop at a time.
    /// </summary>
    Task<IReadOnlyList<Domain.BillOfMaterials.BillOfMaterials>> ListVersionHistoryAsync(Guid companyId, Guid billOfMaterialsId, CancellationToken cancellationToken = default);
}
