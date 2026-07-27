using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Contracts;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.RejectSalesReturn;

public sealed record RejectSalesReturnCommand(Guid CompanyId, Guid SalesReturnId, string RejectionReason)
    : ICommand<SalesReturnDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.return.reject" };
    public string EntityType => nameof(Domain.Returns.SalesReturn);
    public Guid EntityId => SalesReturnId;
    public string Action => "Rejected";
}
