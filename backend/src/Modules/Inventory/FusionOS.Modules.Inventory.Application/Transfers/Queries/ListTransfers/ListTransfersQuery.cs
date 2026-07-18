using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;

namespace FusionOS.Modules.Inventory.Application.Transfers.Queries.ListTransfers;

public sealed record ListTransfersQuery(Guid CompanyId, Guid? ProductId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<TransferDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.transfer.read" };
}
