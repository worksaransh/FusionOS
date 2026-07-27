using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Commands.CompleteOnboardingTask;

public sealed class CompleteOnboardingTaskValidator : AbstractValidator<CompleteOnboardingTaskCommand>
{
    public CompleteOnboardingTaskValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkflowId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
