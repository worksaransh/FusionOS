using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CreateVendorReturn;

public sealed record CreateVendorReturnCommand(Guid CompanyId, Guid PurchaseOrderId, Guid ProductId, Guid WarehouseId, decimal Quantity, string Reason)
    : ICommand<VendorReturnDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.vendor-return.create" };
    public string EntityType => nameof(Domain.VendorReturns.VendorReturn);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
