using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.ConvertRfqToPurchaseOrder;

public sealed record ConvertRfqToPurchaseOrderCommand(Guid CompanyId, Guid RfqId)
    : ICommand<PurchaseOrderDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.rfq.convert" };
    public string EntityType => nameof(Domain.Rfqs.RequestForQuotation);
    public Guid EntityId => RfqId;
    public string Action => "Converted";
}
