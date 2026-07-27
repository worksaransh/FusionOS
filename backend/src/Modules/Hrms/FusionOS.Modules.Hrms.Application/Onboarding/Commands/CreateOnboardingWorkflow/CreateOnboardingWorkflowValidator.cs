using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Commands.CreateOnboardingWorkflow;

public sealed class CreateOnboardingWorkflowValidator : AbstractValidator<CreateOnboardingWorkflowCommand>
{
    public CreateOnboardingWorkflowValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.TaskTitles).NotEmpty().WithMessage("At least one checklist task is required.");
        RuleForEach(x => x.TaskTitles).NotEmpty().MaximumLength(200);
    }
}
