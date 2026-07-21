using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Payroll.Commands.CreatePayrollDraft;

public sealed class CreatePayrollDraftValidator : AbstractValidator<CreatePayrollDraftCommand>
{
    public CreatePayrollDraftValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.PeriodMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.PeriodYear).GreaterThanOrEqualTo(2000);
        RuleFor(x => x.BaseSalary).GreaterThan(0);
    }
}
