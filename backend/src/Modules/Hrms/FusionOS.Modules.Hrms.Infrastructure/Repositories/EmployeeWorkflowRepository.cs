using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Repositories;

/// <summary>
/// Backs IEmployeeWorkflowRepository over HrmsDbContext.Set&lt;EmployeeWorkflow&gt;() rather than
/// a named DbSet property — HrmsDbContext.cs was left untouched for this pass (owned by the
/// orchestrating session); see EmployeeWorkflowConfiguration's own doc comment for why that's safe.
/// </summary>
public sealed class EmployeeWorkflowRepository : IEmployeeWorkflowRepository
{
    private readonly HrmsDbContext _context;

    public EmployeeWorkflowRepository(HrmsDbContext context) => _context = context;

    private DbSet<EmployeeWorkflow> Workflows => _context.Set<EmployeeWorkflow>();

    public Task<EmployeeWorkflow?> GetByIdAsync(Guid companyId, Guid workflowId, WorkflowKind kind, CancellationToken cancellationToken = default) =>
        Workflows.FirstOrDefaultAsync(w => w.CompanyId == companyId && w.Id == workflowId && w.Kind == kind, cancellationToken);

    public async Task AddAsync(EmployeeWorkflow workflow, CancellationToken cancellationToken = default) =>
        await Workflows.AddAsync(workflow, cancellationToken);

    public async Task<IReadOnlyList<EmployeeWorkflow>> ListAsync(Guid companyId, WorkflowKind kind, Guid? employeeId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, kind, employeeId)
            .OrderByDescending(w => w.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, WorkflowKind kind, Guid? employeeId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, kind, employeeId).CountAsync(cancellationToken);

    private IQueryable<EmployeeWorkflow> Filtered(Guid companyId, WorkflowKind kind, Guid? employeeId)
    {
        var query = Workflows.Where(w => w.CompanyId == companyId && w.Kind == kind);
        if (employeeId.HasValue)
            query = query.Where(w => w.EmployeeId == employeeId.Value);
        return query;
    }
}
