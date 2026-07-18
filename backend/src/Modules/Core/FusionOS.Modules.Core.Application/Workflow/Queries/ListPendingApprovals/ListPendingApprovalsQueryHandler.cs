using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Queries.GetApprovalRequest;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Workflow.Queries.ListPendingApprovals;

public sealed class ListPendingApprovalsQueryHandler : IRequestHandler<ListPendingApprovalsQuery, PagedResult<ApprovalRequestDto>>
{
    private readonly IApprovalRequestRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public ListPendingApprovalsQueryHandler(IApprovalRequestRepository repository, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ApprovalRequestDto>> Handle(ListPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var approverUserId = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        var requests = await _repository.ListPendingForApproverAsync(request.CompanyId, approverUserId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountPendingForApproverAsync(request.CompanyId, approverUserId, cancellationToken);

        var dtos = requests.Select(GetApprovalRequestQueryHandler.MapToDto).ToList();
        return new PagedResult<ApprovalRequestDto>(dtos, request.Page, request.PageSize, total);
    }
}
