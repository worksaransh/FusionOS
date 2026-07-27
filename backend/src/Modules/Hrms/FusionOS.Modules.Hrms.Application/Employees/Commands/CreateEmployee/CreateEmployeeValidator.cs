using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.CreateEmployee;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.DepartmentName).MaximumLength(200);
        RuleFor(x => x.DepartmentId).NotEqual(Guid.Empty).When(x => x.DepartmentId.HasValue);
        RuleFor(x => x.ManagerId).NotEqual(Guid.Empty).When(x => x.ManagerId.HasValue);
    }
}
