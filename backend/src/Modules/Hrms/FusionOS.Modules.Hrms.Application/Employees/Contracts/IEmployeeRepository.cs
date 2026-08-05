namespace FusionOS.Modules.Hrms.Application.Employees.Contracts;

public interface IEmployeeRepository
{
    Task<Domain.Employees.Employee?> GetByIdAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Employees.Employee employee, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Employees.Employee>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>This employee's direct reports (other employees whose ManagerId points at them) — backs GetEmployeeOrgChartQuery. Single-level only, not a recursive tree (see that query's own doc comment for why).</summary>
    Task<IReadOnlyList<Domain.Employees.Employee>> ListDirectReportsAsync(Guid companyId, Guid managerId, CancellationToken cancellationToken = default);
}
