using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Queries.ListSuppliers;

public sealed record ListSuppliersQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25) : IQuery<PagedResult<SupplierDto>>;
