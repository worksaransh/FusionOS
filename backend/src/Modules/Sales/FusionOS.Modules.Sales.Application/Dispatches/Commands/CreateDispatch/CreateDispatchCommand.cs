using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Domain.Dispatches;

namespace FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;

/// <summary>
/// PickListId (2026-07-21 Dispatch-PickList link follow-up) is optional and defaults to null —
/// an opaque, unvalidated cross-module reference into Warehouse's PickList aggregate, same
/// convention as Dispatch.cs's own doc comment explains. Added as the last positional parameter
/// with a default so every existing caller (the controller, every test) keeps compiling unchanged.
/// </summary>
public sealed record CreateDispatchCommand(Guid CompanyId, Guid SalesOrderId, Guid WarehouseId, IReadOnlyList<DispatchLineInput> Lines, Guid? PickListId = null)
    : ICommand<DispatchDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.dispatch.create" };
    public string EntityType => nameof(Domain.Dispatches.Dispatch);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
