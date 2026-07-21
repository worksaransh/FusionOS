using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.AssignAttributeValueToVariant;

public sealed class AssignAttributeValueToVariantValidator : AbstractValidator<AssignAttributeValueToVariantCommand>
{
    public AssignAttributeValueToVariantValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.AttributeValueId).NotEmpty();
    }
}
