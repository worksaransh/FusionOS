namespace FusionOS.Modules.Core.Application.Workflow.Contracts;

public sealed record ApprovalStepDto(Guid Id, int StepNumber, Guid ApproverUserId, string Decision, DateTimeOffset? DecidedAt, string? Comments);

public sealed record ApprovalRequestDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    Guid RequestedBy,
    string Status,
    int CurrentStepNumber,
    IReadOnlyList<ApprovalStepDto> Steps,
    DateTimeOffset CreatedAt);
