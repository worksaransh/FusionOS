using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.DeactivateTaxRate;

/// <summary>Soft-deactivate only — never a real delete (a tax rate may already be referenced by historical postings once line-level wiring exists).</summary>
public sealed record DeactivateTaxRateCommand(Guid CompanyId, Guid TaxRateId)
    : ICommand<TaxRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.deactivate" };
    public string EntityType => nameof(Domain.TaxRates.TaxRate);
    public Guid EntityId => TaxRateId;
    public string Action => "Deactivated";
}
