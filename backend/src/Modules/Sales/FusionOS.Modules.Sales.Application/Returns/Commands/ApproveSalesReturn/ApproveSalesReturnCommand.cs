using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Contracts;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.ApproveSalesReturn;

public sealed record ApproveSalesReturnCommand(Guid CompanyId, Guid SalesReturnId)
    : ICommand<SalesReturnDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.return.approve" };
    public string EntityType => nameof(Domain.Returns.SalesReturn);
    public Guid EntityId => SalesReturnId;
    public string Action => "Approved";
}
