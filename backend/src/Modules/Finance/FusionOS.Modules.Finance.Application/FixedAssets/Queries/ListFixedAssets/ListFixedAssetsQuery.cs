using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Queries.ListFixedAssets;

/// <summary>IsDisposed/IsActive are optional filters — null lists every fixed asset for the company, true/false narrows to only disposed/active ones, same optional-filter shape ListBankStatementLinesQuery uses for IsReconciled.</summary>
public sealed record ListFixedAssetsQuery(Guid CompanyId, bool? IsDisposed = null, bool? IsActive = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<FixedAssetDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.read" };
}
