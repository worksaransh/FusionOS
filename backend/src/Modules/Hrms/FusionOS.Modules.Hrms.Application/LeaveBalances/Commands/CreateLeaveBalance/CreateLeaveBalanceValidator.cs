using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.CreateLeaveBalance;

public sealed class CreateLeaveBalanceValidator : AbstractValidator<CreateLeaveBalanceCommand>
{
    public CreateLeaveBalanceValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Year).GreaterThanOrEqualTo(2000);
        RuleFor(x => x.EntitledDays).GreaterThanOrEqualTo(0);
    }
}
