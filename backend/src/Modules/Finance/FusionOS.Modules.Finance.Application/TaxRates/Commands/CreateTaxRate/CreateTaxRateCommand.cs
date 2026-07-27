using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;

/// <summary>
/// GstComponent travels as a string (default "NotApplicable") and is parsed
/// in the handler via Enum.TryParse - same convention as this codebase's
/// other enum-over-the-wire commands (e.g. Finance's own AccountType on
/// CreateAccountCommand), so a caller in a non-GST jurisdiction never has to
/// think about it.
/// </summary>
public sealed record CreateTaxRateCommand(Guid CompanyId, Guid TaxJurisdictionId, string Code, string Name, decimal Percentage, string GstComponent = "NotApplicable")
    : ICommand<TaxRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-rate.create" };
    public string EntityType => nameof(Domain.TaxRates.TaxRate);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
