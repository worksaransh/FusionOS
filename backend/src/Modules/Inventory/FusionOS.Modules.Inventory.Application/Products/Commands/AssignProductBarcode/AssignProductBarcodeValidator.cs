using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.AssignProductBarcode;

public sealed class AssignProductBarcodeValidator : AbstractValidator<AssignProductBarcodeCommand>
{
    public AssignProductBarcodeValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        // Barcode itself is optional (null/blank clears it — see Product.AssignBarcode), so no
        // NotEmpty rule; only bound the length when a value is actually supplied.
        RuleFor(x => x.Barcode).MaximumLength(64).When(x => !string.IsNullOrWhiteSpace(x.Barcode));
    }
}
