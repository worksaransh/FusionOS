using FluentValidation;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CreateVendorReturn;

public sealed class CreateVendorReturnValidator : AbstractValidator<CreateVendorReturnCommand>
{
    public CreateVendorReturnValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
