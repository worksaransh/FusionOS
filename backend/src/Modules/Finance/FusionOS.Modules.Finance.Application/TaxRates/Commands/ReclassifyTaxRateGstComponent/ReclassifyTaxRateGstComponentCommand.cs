using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.ReclassifyTaxRateGstComponent;

/// <summary>GstComponent carried as string-over-the-wire, same convention as CreateTaxRateCommand. Reuses finance.tax-rate.update — reclassifying is metadata correction, not a pricing change, but still an update to the same aggregate.</summary>
public sealed record ReclassifyTaxRateGstComponentCommand(Guid CompanyId, Guid TaxRateId, string GstComponent)
    : ICommand<TaxRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.update" };
    public string EntityType => nameof(Domain.TaxRates.TaxRate);
    public Guid EntityId => TaxRateId;
    public string Action => "ReclassifiedGstComponent";
}
