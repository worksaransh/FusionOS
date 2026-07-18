using FluentValidation;

namespace FusionOS.Modules.Core.Application.Workflow.Commands.CreateApprovalRequest;

public sealed class CreateApprovalRequestValidator : AbstractValidator<CreateApprovalRequestCommand>
{
    public CreateApprovalRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.ApproverUserIds).NotEmpty().WithMessage("At least one approval step is required.");
        RuleForEach(x => x.ApproverUserIds).NotEmpty().WithMessage("Every approver user id must be a real id.");
    }
}
