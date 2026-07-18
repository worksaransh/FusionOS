using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.DeactivateTaxJurisdiction;

/// <summary>Soft-deactivate only — never a real delete (a jurisdiction may already be referenced by TaxRate children, and TaxRate itself may already be referenced by historical postings once line-level wiring exists).</summary>
public sealed record DeactivateTaxJurisdictionCommand(Guid CompanyId, Guid TaxJurisdictionId)
    : ICommand<TaxJurisdictionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-jurisdiction.deactivate" };
    public string EntityType => nameof(Domain.TaxJurisdictions.TaxJurisdiction);
    public Guid EntityId => TaxJurisdictionId;
    public string Action => "Deactivated";
}
