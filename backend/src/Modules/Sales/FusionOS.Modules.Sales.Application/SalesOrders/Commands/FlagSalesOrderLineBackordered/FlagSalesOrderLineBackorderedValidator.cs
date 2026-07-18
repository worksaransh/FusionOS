using FluentValidation;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.FlagSalesOrderLineBackordered;

public sealed class FlagSalesOrderLineBackorderedValidator : AbstractValidator<FlagSalesOrderLineBackorderedCommand>
{
    public FlagSalesOrderLineBackorderedValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.BackorderedQuantity).GreaterThan(0);
    }
}
