using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.DeactivateShelf;

public sealed class DeactivateShelfValidator : AbstractValidator<DeactivateShelfCommand>
{
    public DeactivateShelfValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
