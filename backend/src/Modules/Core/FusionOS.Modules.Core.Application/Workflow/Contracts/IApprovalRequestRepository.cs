namespace FusionOS.Modules.Core.Application.Workflow.Contracts;

public interface IApprovalRequestRepository
{
    Task AddAsync(Domain.Workflow.ApprovalRequest request, CancellationToken cancellationToken = default);

    Task<Domain.Workflow.ApprovalRequest?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Every request whose *current* step's approver is this user — i.e. genuinely actionable by them right now, not every step they'll ever be asked to decide.</summary>
    Task<IReadOnlyList<Domain.Workflow.ApprovalRequest>> ListPendingForApproverAsync(Guid companyId, Guid approverUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountPendingForApproverAsync(Guid companyId, Guid approverUserId, CancellationToken cancellationToken = default);
}
