using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.UpdateTaxJurisdiction;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateCostCenterCommand.</summary>
public sealed record UpdateTaxJurisdictionCommand(Guid CompanyId, Guid TaxJurisdictionId, string Name)
    : ICommand<TaxJurisdictionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-jurisdiction.update" };
    public string EntityType => nameof(Domain.TaxJurisdictions.TaxJurisdiction);
    public Guid EntityId => TaxJurisdictionId;
    public string Action => "Updated";
}
