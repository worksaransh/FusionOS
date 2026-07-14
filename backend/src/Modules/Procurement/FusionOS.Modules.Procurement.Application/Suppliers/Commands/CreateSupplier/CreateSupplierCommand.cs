using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.CreateSupplier;

public sealed record CreateSupplierCommand(Guid CompanyId, string Name, string Code, string? ContactEmail, string? ContactPhone)
    : ICommand<SupplierDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.supplier.create" };
    public string EntityType => nameof(Domain.Suppliers.Supplier);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
