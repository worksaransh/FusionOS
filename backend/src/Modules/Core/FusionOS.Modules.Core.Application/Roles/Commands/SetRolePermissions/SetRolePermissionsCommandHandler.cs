using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Roles.Commands.SetRolePermissions;

public sealed class SetRolePermissionsCommandHandler : IRequestHandler<SetRolePermissionsCommand, IReadOnlyList<string>>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public SetRolePermissionsCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<string>> Handle(SetRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _users.GetRoleByIdAsync(request.RoleId, request.CompanyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{request.RoleId}' was not found.");

        // PermissionCatalog is meant to be the single source of truth for every
        // permission code (see its own class doc comment) - without this check,
        // a role could be assigned an arbitrary string that IRequirePermission
        // would never grant, silently undermining that guarantee.
        var validCodes = PermissionCatalog.All.Select(p => p.Code).ToHashSet();
        var unknownCodes = request.PermissionCodes.Where(code => !validCodes.Contains(code)).ToList();
        if (unknownCodes.Count > 0)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.PermissionCodes),
                    $"Unknown permission code(s): {string.Join(", ", unknownCodes)}."),
            });
        }

        await _users.SetRolePermissionsAsync(role.Id, request.PermissionCodes, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var codes = await _users.GetRolePermissionCodesAsync(role.Id, cancellationToken);
        return codes.ToList();
    }
}
