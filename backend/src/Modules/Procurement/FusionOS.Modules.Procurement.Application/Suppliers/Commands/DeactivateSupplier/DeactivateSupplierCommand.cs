using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.DeactivateSupplier;

/// <summary>Soft-deactivate only — never a real delete (a Supplier may be referenced by historical Purchase Orders).</summary>
public sealed record DeactivateSupplierCommand(Guid CompanyId, Guid SupplierId)
    : ICommand<SupplierDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.supplier.deactivate" };
    public string EntityType => nameof(Domain.Suppliers.Supplier);
    public Guid EntityId => SupplierId;
    public string Action => "Deactivated";
}
