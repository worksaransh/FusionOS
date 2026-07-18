using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.AddProductVariant;

public sealed class AddProductVariantValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.VariantSku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Attributes).NotEmpty().MaximumLength(500);
    }
}
