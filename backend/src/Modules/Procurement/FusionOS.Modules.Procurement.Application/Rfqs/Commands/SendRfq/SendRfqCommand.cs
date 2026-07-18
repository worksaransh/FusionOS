using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.SendRfq;

public sealed record SendRfqCommand(Guid CompanyId, Guid RfqId)
    : ICommand<RfqDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.rfq.send" };
    public string EntityType => nameof(Domain.Rfqs.RequestForQuotation);
    public Guid EntityId => RfqId;
    public string Action => "Sent";
}
