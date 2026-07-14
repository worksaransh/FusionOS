using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;

public sealed class CreateGoodsReceiptValidator : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ZoneId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A goods receipt must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.QuantityReceived).GreaterThan(0);
            line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0).When(l => l.UnitCost.HasValue);
        });
    }
}
