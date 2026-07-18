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

        await _users.SetRolePermissionsAsync(role.Id, request.PermissionCodes, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var codes = await _users.GetRolePermissionCodesAsync(role.Id, cancellationToken);
        return codes.ToList();
    }
}
