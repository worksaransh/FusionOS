using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;

namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Queries.ListSupplierContracts;

public sealed record ListSupplierContractsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<SupplierContractDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.contract.read" };
}
