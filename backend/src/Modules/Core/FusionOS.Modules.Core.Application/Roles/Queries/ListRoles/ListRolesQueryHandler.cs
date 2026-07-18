using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Roles.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Roles.Queries.ListRoles;

public sealed class ListRolesQueryHandler : IRequestHandler<ListRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IUserRepository _users;

    public ListRolesQueryHandler(IUserRepository users) => _users = users;

    public async Task<IReadOnlyList<RoleDto>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _users.ListRolesByCompanyAsync(request.CompanyId, request.Search, cancellationToken);
        return roles.Select(r => new RoleDto(r.Id, r.Name, r.IsSystemRole)).ToList();
    }
}
