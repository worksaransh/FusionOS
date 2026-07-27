using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Tasks.Commands.CreateFollowUpTask;

public sealed class CreateFollowUpTaskValidator : AbstractValidator<CreateFollowUpTaskCommand>
{
    public CreateFollowUpTaskValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
    }
}
