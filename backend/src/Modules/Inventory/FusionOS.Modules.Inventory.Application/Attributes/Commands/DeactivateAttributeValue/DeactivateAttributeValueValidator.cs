using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeValue;

public sealed class DeactivateAttributeValueValidator : AbstractValidator<DeactivateAttributeValueCommand>
{
    public DeactivateAttributeValueValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AttributeValueId).NotEmpty();
    }
}
