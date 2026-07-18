using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.DeactivateWarehouse;

public sealed class DeactivateWarehouseValidator : AbstractValidator<DeactivateWarehouseCommand>
{
    public DeactivateWarehouseValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
