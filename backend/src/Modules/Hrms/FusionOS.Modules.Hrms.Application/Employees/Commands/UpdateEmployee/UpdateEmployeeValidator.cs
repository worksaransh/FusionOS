using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.UpdateEmployee;

public sealed class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.DepartmentId).NotEqual(Guid.Empty).When(x => x.DepartmentId.HasValue);
        RuleFor(x => x.ManagerId).NotEqual(Guid.Empty).When(x => x.ManagerId.HasValue);
    }
}
