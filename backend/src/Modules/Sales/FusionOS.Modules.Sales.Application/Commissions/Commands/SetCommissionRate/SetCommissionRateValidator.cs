using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Commissions.Commands.SetCommissionRate;

public sealed class SetCommissionRateValidator : AbstractValidator<SetCommissionRateCommand>
{
    public SetCommissionRateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RatePercentage).InclusiveBetween(0, 100);
    }
}
