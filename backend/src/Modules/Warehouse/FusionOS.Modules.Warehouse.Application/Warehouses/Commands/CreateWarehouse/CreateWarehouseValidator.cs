using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.CreateWarehouse;

public sealed class CreateWarehouseValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Address).MaximumLength(300);
    }
}
