using FluentValidation;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.UpdateFixedAsset;

public sealed class UpdateFixedAssetValidator : AbstractValidator<UpdateFixedAssetCommand>
{
    public UpdateFixedAssetValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FixedAssetId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
