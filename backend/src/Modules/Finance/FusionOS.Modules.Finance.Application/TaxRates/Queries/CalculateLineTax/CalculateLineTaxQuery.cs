using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Queries.CalculateLineTax;

/// <summary>
/// The one piece of actual "apply a tax rate to an amount" behavior this slice
/// ships — a pure, on-demand lookup-and-multiply, deliberately modelled on
/// ExchangeRates.ConvertAmount rather than a service class. It computes the tax
/// for a single net line amount against one TaxRate; it does NOT itself write the
/// result onto any transactional line. SalesInvoiceLine/PurchaseOrderLine now
/// carry an optional TaxRateId + a stored TaxAmount, and the caller supplies that
/// TaxAmount (obtained via this query) when creating the line — the same
/// separation ConvertAmount keeps between "compute a conversion" and "post a
/// transaction". Gated by the same finance.tax-rate.read permission as the other
/// TaxRate reads: it reads rate data, it doesn't change any row.
/// </summary>
public sealed record CalculateLineTaxQuery(Guid CompanyId, Guid TaxRateId, decimal NetAmount)
    : IQuery<LineTaxResultDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.read" };
}
