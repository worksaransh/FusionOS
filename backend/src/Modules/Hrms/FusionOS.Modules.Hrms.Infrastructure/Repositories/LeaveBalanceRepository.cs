using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using FusionOS.Modules.Hrms.Domain.LeaveBalances;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Repositories;

/// <summary>
/// Queries LeaveBalance via HrmsDbContext.Set&lt;LeaveBalance&gt;() rather than a discrete
/// DbSet&lt;LeaveBalance&gt; property — HrmsDbContext.cs is owned by a separate, concurrent
/// wiring pass (DbSet property + module registration) and deliberately not touched here;
/// EF Core tracks and queries the entity correctly via the generic Set&lt;T&gt;() accessor as
/// long as it's included in the model (LeaveBalanceConfiguration is picked up by
/// HrmsDbContext's existing ApplyConfigurationsFromAssembly call), so this works either way.
/// </summary>
public sealed class LeaveBalanceRepository : ILeaveBalanceRepository
{
    private readonly HrmsDbContext _context;

    public LeaveBalanceRepository(HrmsDbContext context) => _context = context;

    private DbSet<LeaveBalance> Balances => _context.Set<LeaveBalance>();

    public Task<LeaveBalance?> GetByIdAsync(Guid companyId, Guid leaveBalanceId, CancellationToken cancellationToken = default) =>
        Balances.FirstOrDefaultAsync(b => b.CompanyId == companyId && b.Id == leaveBalanceId, cancellationToken);

    public Task<LeaveBalance?> GetAsync(Guid companyId, Guid employeeId, LeaveType leaveType, int year, CancellationToken cancellationToken = default) =>
        Balances.FirstOrDefaultAsync(
            b => b.CompanyId == companyId && b.EmployeeId == employeeId && b.LeaveType == leaveType && b.Year == year,
            cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid employeeId, LeaveType leaveType, int year, CancellationToken cancellationToken = default) =>
        Balances.AnyAsync(
            b => b.CompanyId == companyId && b.EmployeeId == employeeId && b.LeaveType == leaveType && b.Year == year,
            cancellationToken);

    public async Task AddAsync(LeaveBalance balance, CancellationToken cancellationToken = default) =>
        await Balances.AddAsync(balance, cancellationToken);

    public async Task<IReadOnlyList<LeaveBalance>> ListForEmployeeAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default) =>
        await Balances
            .Where(b => b.CompanyId == companyId && b.EmployeeId == employeeId)
            .OrderByDescending(b => b.Year)
            .ThenBy(b => b.LeaveType)
            .ToListAsync(cancellationToken);
}
