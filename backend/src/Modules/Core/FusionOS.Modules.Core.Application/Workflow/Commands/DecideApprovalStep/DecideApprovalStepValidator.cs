using FluentValidation;

namespace FusionOS.Modules.Core.Application.Workflow.Commands.DecideApprovalStep;

public sealed class DecideApprovalStepValidator : AbstractValidator<DecideApprovalStepCommand>
{
    public DecideApprovalStepValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ApprovalRequestId).NotEmpty();
        RuleFor(x => x.Comments).MaximumLength(2000);
    }
}
