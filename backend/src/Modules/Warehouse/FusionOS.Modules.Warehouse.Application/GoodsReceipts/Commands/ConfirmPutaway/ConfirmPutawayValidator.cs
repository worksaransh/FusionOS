using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.ConfirmPutaway;

public sealed class ConfirmPutawayValidator : AbstractValidator<ConfirmPutawayCommand>
{
    public ConfirmPutawayValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.GoodsReceiptId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
        RuleFor(x => x.BinId).NotEmpty();
    }
}
