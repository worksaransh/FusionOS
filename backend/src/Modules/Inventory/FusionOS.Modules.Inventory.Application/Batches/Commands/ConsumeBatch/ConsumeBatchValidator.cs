using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.ConsumeBatch;

public sealed class ConsumeBatchValidator : AbstractValidator<ConsumeBatchCommand>
{
    public ConsumeBatchValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BatchId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
