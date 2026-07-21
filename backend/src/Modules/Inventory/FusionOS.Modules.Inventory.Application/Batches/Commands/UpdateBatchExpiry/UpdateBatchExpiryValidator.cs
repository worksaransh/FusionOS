using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.UpdateBatchExpiry;

public sealed class UpdateBatchExpiryValidator : AbstractValidator<UpdateBatchExpiryCommand>
{
    public UpdateBatchExpiryValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BatchId).NotEmpty();
    }
}
