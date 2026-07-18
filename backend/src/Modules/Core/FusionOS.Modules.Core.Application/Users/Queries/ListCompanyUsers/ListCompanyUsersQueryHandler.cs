using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Users.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Users.Queries.ListCompanyUsers;

public sealed class ListCompanyUsersQueryHandler : IRequestHandler<ListCompanyUsersQuery, IReadOnlyList<CompanyUserDto>>
{
    private readonly IUserRepository _users;

    public ListCompanyUsersQueryHandler(IUserRepository users) => _users = users;

    public async Task<IReadOnlyList<CompanyUserDto>> Handle(ListCompanyUsersQuery request, CancellationToken cancellationToken)
    {
        var rows = await _users.ListCompanyUsersAsync(request.CompanyId, request.Search, cancellationToken);
        return rows.Select(r => new CompanyUserDto(r.UserId, r.Email, r.FullName, r.RoleId, r.RoleName)).ToList();
    }
}
