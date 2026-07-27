using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Commands.CreateOffboardingWorkflow;

public sealed class CreateOffboardingWorkflowValidator : AbstractValidator<CreateOffboardingWorkflowCommand>
{
    public CreateOffboardingWorkflowValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.TaskTitles).NotEmpty().WithMessage("At least one checklist task is required.");
        RuleForEach(x => x.TaskTitles).NotEmpty().MaximumLength(200);
    }
}
