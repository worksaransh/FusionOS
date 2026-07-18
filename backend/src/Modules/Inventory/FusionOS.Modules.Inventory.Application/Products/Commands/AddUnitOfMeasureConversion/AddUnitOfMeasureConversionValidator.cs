using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.AddUnitOfMeasureConversion;

public sealed class AddUnitOfMeasureConversionValidator : AbstractValidator<AddUnitOfMeasureConversionCommand>
{
    public AddUnitOfMeasureConversionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.AlternateUnitOfMeasure).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ConversionFactor).GreaterThan(0m).WithMessage("Conversion factor must be greater than zero.");
    }
}
