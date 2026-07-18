using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.AssignPickList;

public sealed class AssignPickListValidator : AbstractValidator<AssignPickListCommand>
{
    public AssignPickListValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AssignedToUserId).NotEmpty();
    }
}
