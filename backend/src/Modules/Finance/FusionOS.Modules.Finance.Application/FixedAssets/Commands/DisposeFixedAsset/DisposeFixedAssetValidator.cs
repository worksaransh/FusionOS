using FluentValidation;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;

public sealed class DisposeFixedAssetValidator : AbstractValidator<DisposeFixedAssetCommand>
{
    public DisposeFixedAssetValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FixedAssetId).NotEmpty();
        RuleFor(x => x.DisposedDate).NotEmpty();
    }
}
