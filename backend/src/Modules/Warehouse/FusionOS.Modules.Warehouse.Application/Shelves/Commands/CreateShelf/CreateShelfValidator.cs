using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.CreateShelf;

public sealed class CreateShelfValidator : AbstractValidator<CreateShelfCommand>
{
    public CreateShelfValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.RackId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
