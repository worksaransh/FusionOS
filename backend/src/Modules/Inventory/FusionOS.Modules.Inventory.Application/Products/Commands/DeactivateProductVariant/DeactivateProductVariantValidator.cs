using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProductVariant;

public sealed class DeactivateProductVariantValidator : AbstractValidator<DeactivateProductVariantCommand>
{
    public DeactivateProductVariantValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantId).NotEmpty();
    }
}
