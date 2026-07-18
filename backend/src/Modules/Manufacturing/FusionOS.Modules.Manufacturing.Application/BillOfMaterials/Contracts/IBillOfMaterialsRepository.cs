namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

public interface IBillOfMaterialsRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Domain.BillOfMaterials.BillOfMaterials?> GetByIdAsync(Guid companyId, Guid billOfMaterialsId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.BillOfMaterials.BillOfMaterials billOfMaterials, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.BillOfMaterials.BillOfMaterials>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
