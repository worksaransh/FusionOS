using FusionOS.Modules.Core.Application.Workflow.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Workflow.Queries.GetApprovalRequest;

public sealed class GetApprovalRequestQueryHandler : IRequestHandler<GetApprovalRequestQuery, ApprovalRequestDto>
{
    private readonly IApprovalRequestRepository _repository;

    public GetApprovalRequestQueryHandler(IApprovalRequestRepository repository) => _repository = repository;

    public async Task<ApprovalRequestDto> Handle(GetApprovalRequestQuery request, CancellationToken cancellationToken)
    {
        var approvalRequest = await _repository.GetByIdAsync(request.CompanyId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Approval request {request.Id} not found.");

        return MapToDto(approvalRequest);
    }

    internal static ApprovalRequestDto MapToDto(Domain.Workflow.ApprovalRequest request) => new(
        request.Id,
        request.EntityType,
        request.EntityId,
        request.RequestedBy,
        request.Status.ToString(),
        request.CurrentStepNumber,
        request.Steps.Select(s => new ApprovalStepDto(s.Id, s.StepNumber, s.ApproverUserId, s.Decision.ToString(), s.DecidedAt, s.Comments)).ToList(),
        request.CreatedAt);
}
