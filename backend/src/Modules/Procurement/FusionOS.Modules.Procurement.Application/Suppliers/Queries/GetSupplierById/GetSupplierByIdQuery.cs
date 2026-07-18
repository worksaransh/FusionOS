using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Queries.GetSupplierById;

/// <summary>Tenant-scoped single-supplier lookup — wires up SuppliersController's previously dead GetById stub. Read-gated the same as ListSuppliersQuery.</summary>
public sealed record GetSupplierByIdQuery(Guid CompanyId, Guid SupplierId)
    : IQuery<SupplierDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.supplier.read" };
}
