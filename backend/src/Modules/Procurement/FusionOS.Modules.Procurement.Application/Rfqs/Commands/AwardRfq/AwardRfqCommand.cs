using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.AwardRfq;

public sealed record AwardRfqCommand(Guid CompanyId, Guid RfqId, Guid SupplierQuoteId)
    : ICommand<RfqDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.rfq.award" };
    public string EntityType => nameof(Domain.Rfqs.RequestForQuotation);
    public Guid EntityId => RfqId;
    public string Action => "Awarded";
}
