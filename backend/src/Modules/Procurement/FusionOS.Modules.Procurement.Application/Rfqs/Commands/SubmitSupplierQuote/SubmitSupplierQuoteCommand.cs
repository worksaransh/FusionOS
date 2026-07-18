using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.SubmitSupplierQuote;

public sealed record SubmitSupplierQuoteCommand(Guid CompanyId, Guid RfqId, Guid SupplierId, IReadOnlyList<SupplierQuoteLineInput> Lines)
    : ICommand<RfqDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.rfq.submit-quote" };
    public string EntityType => nameof(Domain.Rfqs.RequestForQuotation);
    public Guid EntityId => RfqId;
    public string Action => "SupplierQuoteSubmitted";
}
