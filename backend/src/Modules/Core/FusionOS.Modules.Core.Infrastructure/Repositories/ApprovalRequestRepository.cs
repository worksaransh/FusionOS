using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Domain.Workflow;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class ApprovalRequestRepository : IApprovalRequestRepository
{
    private readonly CoreDbContext _context;

    public ApprovalRequestRepository(CoreDbContext context) => _context = context;

    public async Task AddAsync(ApprovalRequest request, CancellationToken cancellationToken = default) =>
        await _context.ApprovalRequests.AddAsync(request, cancellationToken);

    public Task<ApprovalRequest?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.ApprovalRequests
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ApprovalRequest>> ListPendingForApproverAsync(Guid companyId, Guid approverUserId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, approverUserId)
            .Include(x => x.Steps)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountPendingForApproverAsync(Guid companyId, Guid approverUserId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, approverUserId).CountAsync(cancellationToken);

    // A request is "pending for this approver" only if they're the approver of
    // whatever step is currently actionable (CurrentStepNumber) — not merely
    // listed anywhere in the chain, since a later step's approver has nothing
    // to do until every earlier step is approved.
    private IQueryable<ApprovalRequest> Filtered(Guid companyId, Guid approverUserId) =>
        _context.ApprovalRequests.Where(r =>
            r.CompanyId == companyId &&
            r.Status == ApprovalStatus.Pending &&
            r.Steps.Any(s => s.StepNumber == r.CurrentStepNumber && s.ApproverUserId == approverUserId));
}
