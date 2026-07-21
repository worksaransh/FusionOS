using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.RemoveAttributeValueFromVariant;

public sealed class RemoveAttributeValueFromVariantValidator : AbstractValidator<RemoveAttributeValueFromVariantCommand>
{
    public RemoveAttributeValueFromVariantValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
        RuleFor(x => x.AssignmentId).NotEmpty();
    }
}
