using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(Guid CompanyId, Guid SupplierId, string Name, string? ContactEmail, string? ContactPhone)
    : ICommand<SupplierDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.supplier.update" };
    public string EntityType => nameof(Domain.Suppliers.Supplier);
    public Guid EntityId => SupplierId;
    public string Action => "Updated";
}
