using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.AdjustLeaveBalance;

public sealed class AdjustLeaveBalanceValidator : AbstractValidator<AdjustLeaveBalanceCommand>
{
    public AdjustLeaveBalanceValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.LeaveBalanceId).NotEmpty();
        RuleFor(x => x.NewEntitledDays).GreaterThanOrEqualTo(0);
    }
}
