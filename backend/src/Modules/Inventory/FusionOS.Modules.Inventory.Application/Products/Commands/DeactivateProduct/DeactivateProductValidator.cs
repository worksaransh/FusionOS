using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProduct;

public sealed class DeactivateProductValidator : AbstractValidator<DeactivateProductCommand>
{
    public DeactivateProductValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
