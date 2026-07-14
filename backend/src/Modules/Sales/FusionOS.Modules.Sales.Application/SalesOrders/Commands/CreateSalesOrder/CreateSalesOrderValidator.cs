using FluentValidation;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;

public sealed class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A sales order must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
