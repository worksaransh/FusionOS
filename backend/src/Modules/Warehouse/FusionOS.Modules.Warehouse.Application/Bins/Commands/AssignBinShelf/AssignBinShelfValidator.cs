using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.AssignBinShelf;

public sealed class AssignBinShelfValidator : AbstractValidator<AssignBinShelfCommand>
{
    public AssignBinShelfValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BinId).NotEmpty();
        // ShelfId is intentionally allowed to be null/empty — that's how a bin's shelf assignment is cleared.
    }
}
