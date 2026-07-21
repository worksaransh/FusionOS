using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;

namespace FusionOS.Modules.Inventory.Application.Batches.Queries.ListBatchesByProduct;

/// <summary>ExpiringBefore is the expiry-reporting filter — pass e.g. "30 days from now" to find batches about to go out of date.</summary>
public sealed record ListBatchesByProductQuery(Guid CompanyId, Guid ProductId, DateTimeOffset? ExpiringBefore = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BatchDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.batch.read" };
}
