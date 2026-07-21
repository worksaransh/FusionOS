namespace FusionOS.Modules.Core.Application.Branches.Contracts;

public interface IBranchRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Domain.Organizations.Branch?> GetByIdAsync(Guid companyId, Guid branchId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Organizations.Branch branch, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Organizations.Branch>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
