using FluentValidation;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.PostMonthlyDepreciation;

public sealed class PostMonthlyDepreciationValidator : AbstractValidator<PostMonthlyDepreciationCommand>
{
    public PostMonthlyDepreciationValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.FixedAssetId).NotEmpty();
        RuleFor(x => x.DepreciationExpenseAccountId).NotEmpty();
        RuleFor(x => x.PeriodEnd).NotEmpty();
    }
}
