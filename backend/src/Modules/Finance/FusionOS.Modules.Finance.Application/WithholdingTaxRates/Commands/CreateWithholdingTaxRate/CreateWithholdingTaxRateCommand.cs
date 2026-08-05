using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.CreateWithholdingTaxRate;

public sealed record CreateWithholdingTaxRateCommand(Guid CompanyId, string SectionCode, string Description, decimal Percentage)
    : ICommand<WithholdingTaxRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.withholding-tax-rate.create" };
    public string EntityType => nameof(Domain.WithholdingTaxRates.WithholdingTaxRate);
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Created";
}
