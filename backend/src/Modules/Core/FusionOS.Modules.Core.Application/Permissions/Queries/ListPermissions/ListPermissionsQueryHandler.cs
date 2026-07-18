using FusionOS.Modules.Core.Application.Auth;
using FusionOS.Modules.Core.Application.Permissions.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Permissions.Queries.ListPermissions;

public sealed class ListPermissionsQueryHandler : IRequestHandler<ListPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public Task<IReadOnlyList<PermissionDto>> Handle(ListPermissionsQuery request, CancellationToken cancellationToken)
    {
        var catalog = PermissionCatalog.All.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            catalog = catalog.Where(p =>
                p.Module.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.Code.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        IReadOnlyList<PermissionDto> result = catalog
            .Select(p => new PermissionDto(p.Module, p.Code, p.Description))
            .OrderBy(p => p.Module).ThenBy(p => p.Code)
            .ToList();

        return Task.FromResult(result);
    }
}
