using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Queries.GetRfqQuoteComparison;

/// <summary>
/// A richer read view over an RFQ's own SupplierQuotes — line-by-line, side-by-side
/// per supplier — for the decision-maker picking who to Award. Gated by the same
/// procurement.rfq.read permission as GetRfqByIdQuery/ListRfqsQuery: this surfaces no
/// data the reader couldn't already reconstruct by reading every SupplierQuote off
/// the plain RfqDto, it is just shaped for comparison instead of per-supplier nesting.
/// </summary>
public sealed record GetRfqQuoteComparisonQuery(Guid CompanyId, Guid RfqId)
    : IQuery<RfqQuoteComparisonDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "procurement.rfq.read" };
}
