using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.UpdateShelf;

public sealed class UpdateShelfValidator : AbstractValidator<UpdateShelfCommand>
{
    public UpdateShelfValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
