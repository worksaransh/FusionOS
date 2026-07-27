using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Commands.CompleteOffboardingTask;

public sealed class CompleteOffboardingTaskValidator : AbstractValidator<CompleteOffboardingTaskCommand>
{
    public CompleteOffboardingTaskValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkflowId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
