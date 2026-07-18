using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.UpdateTaxRate;

/// <summary>Update deliberately excludes Code and TaxJurisdictionId — Code is the immutable business key and TaxJurisdictionId is the rate's parent FK, same immutability rule as UpdateBinCommand.</summary>
public sealed record UpdateTaxRateCommand(Guid CompanyId, Guid TaxRateId, string Name, decimal Percentage)
    : ICommand<TaxRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.update" };
    public string EntityType => nameof(Domain.TaxRates.TaxRate);
    public Guid EntityId => TaxRateId;
    public string Action => "Updated";
}
