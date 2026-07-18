using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.CreatePickList;

public sealed class CreatePickListValidator : AbstractValidator<CreatePickListCommand>
{
    public CreatePickListValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.SalesOrderId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A pick list must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.QuantityToPick).GreaterThan(0);
        });
    }
}
