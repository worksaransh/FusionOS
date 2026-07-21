using FluentValidation;
using FusionOS.Modules.Crm.Domain.Activities;

namespace FusionOS.Modules.Crm.Application.Activities.Commands.CreateActivity;

public sealed class CreateActivityValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().Must(v => Enum.TryParse<ActivityType>(v, out _))
            .WithMessage($"Type must be one of: {string.Join(", ", Enum.GetNames<ActivityType>())}.");
        RuleFor(x => x.Notes).NotEmpty().MaximumLength(2000);
    }
}
