using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.CreateProduct;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitOfMeasure).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
