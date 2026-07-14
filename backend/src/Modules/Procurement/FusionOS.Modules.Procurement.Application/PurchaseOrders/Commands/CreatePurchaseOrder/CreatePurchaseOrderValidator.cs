using FluentValidation;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A purchase order must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
