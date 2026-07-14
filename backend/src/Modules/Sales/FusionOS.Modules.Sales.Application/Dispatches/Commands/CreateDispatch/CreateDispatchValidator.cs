using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;

public sealed class CreateDispatchValidator : AbstractValidator<CreateDispatchCommand>
{
    public CreateDispatchValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A dispatch must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.QuantityDispatched).GreaterThan(0);
        });
    }
}
