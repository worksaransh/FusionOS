namespace FusionOS.Modules.Hrms.Application.Payroll.Contracts;

public interface IPayrollRecordRepository
{
    Task<Domain.Payroll.PayrollRecord?> GetByIdAsync(Guid companyId, Guid payrollRecordId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForPeriodAsync(Guid companyId, Guid employeeId, int periodMonth, int periodYear, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Payroll.PayrollRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Payroll.PayrollRecord>> ListAsync(Guid companyId, Guid? employeeId, int? periodMonth, int? periodYear, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? employeeId, int? periodMonth, int? periodYear, CancellationToken cancellationToken = default);
}
