using FluentValidation;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.RecordSparePartUsage;

public sealed class RecordSparePartUsageValidator : AbstractValidator<RecordSparePartUsageCommand>
{
    public RecordSparePartUsageValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MaintenanceRequestId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.QuantityUsed).GreaterThan(0);
        RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0);
    }
}
