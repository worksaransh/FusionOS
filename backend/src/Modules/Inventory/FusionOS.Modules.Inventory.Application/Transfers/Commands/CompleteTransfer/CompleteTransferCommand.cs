using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;

namespace FusionOS.Modules.Inventory.Application.Transfers.Commands.CompleteTransfer;

public sealed record CompleteTransferCommand(Guid CompanyId, Guid TransferId)
    : ICommand<TransferDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.transfer.complete" };
    public string EntityType => nameof(Domain.Transfers.Transfer);
    public Guid EntityId => TransferId;
    public string Action => "Completed";
}
