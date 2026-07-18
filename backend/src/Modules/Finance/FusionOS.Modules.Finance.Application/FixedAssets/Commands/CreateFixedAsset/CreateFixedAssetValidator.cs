using FluentValidation;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;

public sealed class CreateFixedAssetValidator : AbstractValidator<CreateFixedAssetCommand>
{
    public CreateFixedAssetValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AssetAccountId).NotEmpty();
        RuleFor(x => x.AcquisitionDate).NotEmpty();
        RuleFor(x => x.AcquisitionCost).GreaterThan(0);
        RuleFor(x => x.SalvageValue).GreaterThanOrEqualTo(0).LessThan(x => x.AcquisitionCost)
            .WithMessage("Salvage value must be less than acquisition cost.");
        RuleFor(x => x.UsefulLifeMonths).GreaterThan(0);
    }
}
