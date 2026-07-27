using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Contracts;

namespace FusionOS.Modules.Sales.Application.Returns.Queries.GetSalesReturnById;

public sealed record GetSalesReturnByIdQuery(Guid CompanyId, Guid SalesReturnId) : IQuery<SalesReturnDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.return.read" };
}
