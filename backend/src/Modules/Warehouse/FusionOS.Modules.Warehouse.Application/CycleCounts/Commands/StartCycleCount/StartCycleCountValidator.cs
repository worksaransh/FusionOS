using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;

public sealed class StartCycleCountValidator : AbstractValidator<StartCycleCountCommand>
{
    public StartCycleCountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ZoneId).NotEmpty();
        RuleFor(x => x.BinId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.SystemQuantitySnapshot).GreaterThanOrEqualTo(0);
    }
}
