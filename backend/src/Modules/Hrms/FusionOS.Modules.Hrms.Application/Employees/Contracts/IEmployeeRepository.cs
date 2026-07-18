namespace FusionOS.Modules.Hrms.Application.Employees.Contracts;

public interface IEmployeeRepository
{
    Task<Domain.Employees.Employee?> GetByIdAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Employees.Employee employee, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Employees.Employee>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
