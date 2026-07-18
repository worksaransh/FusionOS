using FusionOS.Modules.Core.Application.Auth.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Roles.Queries.GetRolePermissions;

public sealed class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, IReadOnlyList<string>>
{
    private readonly IUserRepository _users;

    public GetRolePermissionsQueryHandler(IUserRepository users) => _users = users;

    public async Task<IReadOnlyList<string>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        var role = await _users.GetRoleByIdAsync(request.RoleId, request.CompanyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{request.RoleId}' was not found.");

        var codes = await _users.GetRolePermissionCodesAsync(role.Id, cancellationToken);
        return codes.ToList();
    }
}
