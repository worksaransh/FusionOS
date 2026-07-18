using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.RemoveUnitOfMeasureConversion;

public sealed class RemoveUnitOfMeasureConversionValidator : AbstractValidator<RemoveUnitOfMeasureConversionCommand>
{
    public RemoveUnitOfMeasureConversionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.AlternateUnitOfMeasure).NotEmpty().MaximumLength(20);
    }
}
