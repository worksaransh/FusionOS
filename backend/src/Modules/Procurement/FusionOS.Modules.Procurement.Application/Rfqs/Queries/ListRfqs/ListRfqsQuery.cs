using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Queries.ListRfqs;

public sealed record ListRfqsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<RfqDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.rfq.read" };
}
