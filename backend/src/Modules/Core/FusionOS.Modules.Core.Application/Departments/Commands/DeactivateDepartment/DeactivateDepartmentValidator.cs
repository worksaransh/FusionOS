using FluentValidation;

namespace FusionOS.Modules.Core.Application.Departments.Commands.DeactivateDepartment;

public sealed class DeactivateDepartmentValidator : AbstractValidator<DeactivateDepartmentCommand>
{
    public DeactivateDepartmentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DepartmentId).NotEmpty();
    }
}
