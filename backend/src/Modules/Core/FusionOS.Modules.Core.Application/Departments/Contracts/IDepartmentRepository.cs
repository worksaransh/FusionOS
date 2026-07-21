namespace FusionOS.Modules.Core.Application.Departments.Contracts;

/// <summary>
/// No CodeExistsAsync here (unlike IBranchRepository/ICostCenterRepository) —
/// DepartmentConfiguration's (CompanyId, Code) index is deliberately
/// non-unique, so a department code may legitimately repeat across branches
/// within the same company.
/// </summary>
public interface IDepartmentRepository
{
    Task<Domain.Organizations.Department?> GetByIdAsync(Guid companyId, Guid departmentId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Organizations.Department department, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Organizations.Department>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
