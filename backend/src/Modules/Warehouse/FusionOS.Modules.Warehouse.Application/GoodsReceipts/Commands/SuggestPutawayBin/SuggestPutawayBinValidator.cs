using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.SuggestPutawayBin;

public sealed class SuggestPutawayBinValidator : AbstractValidator<SuggestPutawayBinCommand>
{
    public SuggestPutawayBinValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.GoodsReceiptId).NotEmpty();
        RuleFor(x => x.LineId).NotEmpty();
    }
}
