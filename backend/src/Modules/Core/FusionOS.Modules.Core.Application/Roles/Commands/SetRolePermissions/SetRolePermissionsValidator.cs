using FluentValidation;
using FusionOS.Modules.Core.Application.Auth;

namespace FusionOS.Modules.Core.Application.Roles.Commands.SetRolePermissions;

public sealed class SetRolePermissionsValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionCodes).NotNull();

        RuleForEach(x => x.PermissionCodes)
            .Must(code => PermissionCatalog.All.Any(p => p.Code == code))
            .WithMessage(code => $"'{code}' is not a known permission code.");
    }
}
