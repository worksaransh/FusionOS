namespace FusionOS.Modules.Core.Application.Departments.Contracts;

public interface IDepartmentRepository
{
    Task<Domain.Organizations.Department?> GetByIdAsync(Guid companyId, Guid departmentId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Organizations.Department department, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Organizations.Department>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
