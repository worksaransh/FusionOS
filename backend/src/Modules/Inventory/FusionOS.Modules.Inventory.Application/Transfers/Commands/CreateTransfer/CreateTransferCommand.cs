using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;

namespace FusionOS.Modules.Inventory.Application.Transfers.Commands.CreateTransfer;

public sealed record CreateTransferCommand(Guid CompanyId, Guid ProductId, Guid SourceWarehouseId, Guid DestinationWarehouseId, decimal Quantity)
    : ICommand<TransferDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.transfer.create" };
    public string EntityType => nameof(Domain.Transfers.Transfer);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
