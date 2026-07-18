using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Transfers.Commands.CreateTransfer;

public sealed class CreateTransferValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.SourceWarehouseId).NotEmpty();
        RuleFor(x => x.DestinationWarehouseId).NotEmpty();
        RuleFor(x => x.DestinationWarehouseId).NotEqual(x => x.SourceWarehouseId).WithMessage("Source and destination warehouses must be different.");
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
