using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.PackPickList;

public sealed class PackPickListValidator : AbstractValidator<PackPickListCommand>
{
    public PackPickListValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
