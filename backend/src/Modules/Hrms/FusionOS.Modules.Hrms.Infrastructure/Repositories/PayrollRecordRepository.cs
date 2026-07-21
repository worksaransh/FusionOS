using FusionOS.Modules.Hrms.Application.Payroll.Contracts;
using FusionOS.Modules.Hrms.Domain.Payroll;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Repositories;

public sealed class PayrollRecordRepository : IPayrollRecordRepository
{
    private readonly HrmsDbContext _context;

    public PayrollRecordRepository(HrmsDbContext context) => _context = context;

    public Task<PayrollRecord?> GetByIdAsync(Guid companyId, Guid payrollRecordId, CancellationToken cancellationToken = default) =>
        _context.PayrollRecords.FirstOrDefaultAsync(p => p.CompanyId == companyId && p.Id == payrollRecordId, cancellationToken);

    public Task<bool> ExistsForPeriodAsync(Guid companyId, Guid employeeId, int periodMonth, int periodYear, CancellationToken cancellationToken = default) =>
        _context.PayrollRecords.AnyAsync(
            p => p.CompanyId == companyId && p.EmployeeId == employeeId && p.PeriodMonth == periodMonth && p.PeriodYear == periodYear,
            cancellationToken);

    public async Task AddAsync(PayrollRecord record, CancellationToken cancellationToken = default) =>
        await _context.PayrollRecords.AddAsync(record, cancellationToken);

    public async Task<IReadOnlyList<PayrollRecord>> ListAsync(Guid companyId, Guid? employeeId, int? periodMonth, int? periodYear, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, employeeId, periodMonth, periodYear)
            .OrderByDescending(p => p.PeriodYear)
            .ThenByDescending(p => p.PeriodMonth)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? employeeId, int? periodMonth, int? periodYear, CancellationToken cancellationToken = default) =>
        Filtered(companyId, employeeId, periodMonth, periodYear).CountAsync(cancellationToken);

    private IQueryable<PayrollRecord> Filtered(Guid companyId, Guid? employeeId, int? periodMonth, int? periodYear)
    {
        var query = _context.PayrollRecords.Where(p => p.CompanyId == companyId);
        if (employeeId.HasValue)
            query = query.Where(p => p.EmployeeId == employeeId.Value);
        if (periodMonth.HasValue)
            query = query.Where(p => p.PeriodMonth == periodMonth.Value);
        if (periodYear.HasValue)
            query = query.Where(p => p.PeriodYear == periodYear.Value);
        return query;
    }
}
