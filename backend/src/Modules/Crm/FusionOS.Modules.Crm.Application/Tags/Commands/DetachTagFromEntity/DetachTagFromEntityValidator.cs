using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Tags.Commands.DetachTagFromEntity;

public sealed class DetachTagFromEntityValidator : AbstractValidator<DetachTagFromEntityCommand>
{
    public DetachTagFromEntityValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TagId).NotEmpty();
        RuleFor(x => x.TargetEntityId).NotEmpty();
        RuleFor(x => x.TargetEntityType).NotEmpty();
    }
}
