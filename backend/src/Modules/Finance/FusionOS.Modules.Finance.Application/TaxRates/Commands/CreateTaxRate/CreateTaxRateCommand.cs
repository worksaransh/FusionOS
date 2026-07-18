using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;

public sealed record CreateTaxRateCommand(Guid CompanyId, Guid TaxJurisdictionId, string Code, string Name, decimal Percentage)
    : ICommand<TaxRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.create" };
    public string EntityType => nameof(Domain.TaxRates.TaxRate);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
