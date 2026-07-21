using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;

namespace FusionOS.Modules.Inventory.Application.Batches.Queries.GetBatchById;

public sealed record GetBatchByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<BatchDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.batch.read" };
}
