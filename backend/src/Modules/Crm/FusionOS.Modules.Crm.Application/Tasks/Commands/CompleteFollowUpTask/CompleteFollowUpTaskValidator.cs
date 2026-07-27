using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Tasks.Commands.CompleteFollowUpTask;

public sealed class CompleteFollowUpTaskValidator : AbstractValidator<CompleteFollowUpTaskCommand>
{
    public CompleteFollowUpTaskValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
    }
}
