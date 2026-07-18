using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;

public sealed record CreateRfqCommand(Guid CompanyId, IReadOnlyList<RfqLineInput> Lines)
    : ICommand<RfqDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.rfq.create" };
    public string EntityType => nameof(Domain.Rfqs.RequestForQuotation);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
