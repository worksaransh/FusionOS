using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.CreateBatch;

public sealed class CreateBatchValidator : AbstractValidator<CreateBatchCommand>
{
    public CreateBatchValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.BatchNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.QuantityReceived).GreaterThan(0);
    }
}
