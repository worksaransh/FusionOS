using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Domain.Dispatches;

namespace FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;

public sealed record CreateDispatchCommand(Guid CompanyId, Guid SalesOrderId, Guid WarehouseId, IReadOnlyList<DispatchLineInput> Lines)
    : ICommand<DispatchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.dispatch.create" };
    public string EntityType => nameof(Domain.Dispatches.Dispatch);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
