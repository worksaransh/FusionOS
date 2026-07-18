using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.RecordCycleCount;

public sealed class RecordCycleCountValidator : AbstractValidator<RecordCycleCountCommand>
{
    public RecordCycleCountValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CountedQuantity).GreaterThanOrEqualTo(0);
    }
}
