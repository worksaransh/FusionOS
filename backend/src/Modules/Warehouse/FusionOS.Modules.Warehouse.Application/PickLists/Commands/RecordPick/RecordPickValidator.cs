using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.RecordPick;

public sealed class RecordPickValidator : AbstractValidator<RecordPickCommand>
{
    public RecordPickValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.QuantityPicked).GreaterThanOrEqualTo(0);
    }
}
