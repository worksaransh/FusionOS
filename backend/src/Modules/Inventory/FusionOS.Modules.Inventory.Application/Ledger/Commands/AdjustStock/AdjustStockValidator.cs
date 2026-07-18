using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Ledger.Commands.AdjustStock;

public sealed class AdjustStockValidator : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.QuantityDelta).NotEqual(0m).WithMessage("Quantity delta cannot be zero.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BatchNumber).MaximumLength(100);
        RuleFor(x => x.SerialNumber).MaximumLength(100);
    }
}
