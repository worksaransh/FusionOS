using FluentValidation;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.ClearSalesOrderLineBackorder;

public sealed class ClearSalesOrderLineBackorderValidator : AbstractValidator<ClearSalesOrderLineBackorderCommand>
{
    public ClearSalesOrderLineBackorderValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
    }
}
