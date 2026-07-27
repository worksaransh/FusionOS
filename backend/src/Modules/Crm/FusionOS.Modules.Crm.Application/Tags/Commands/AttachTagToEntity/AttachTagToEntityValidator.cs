using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Tags.Commands.AttachTagToEntity;

public sealed class AttachTagToEntityValidator : AbstractValidator<AttachTagToEntityCommand>
{
    private static readonly string[] AllowedEntityTypes = { "Lead", "Contact", "Account", "Opportunity" };

    public AttachTagToEntityValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.TagId).NotEmpty();
        RuleFor(x => x.TargetEntityId).NotEmpty();
        RuleFor(x => x.TargetEntityType)
            .NotEmpty()
            .Must(entityType => AllowedEntityTypes.Contains(entityType))
            .WithMessage($"Entity type must be one of: {string.Join(", ", AllowedEntityTypes)}.");
    }
}
