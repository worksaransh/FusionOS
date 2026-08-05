using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.DeactivateWithholdingTaxRate;

public sealed record DeactivateWithholdingTaxRateCommand(Guid CompanyId, Guid Id)
    : ICommand<WithholdingTaxRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.withholding-tax-rate.deactivate" };
    public string EntityType => nameof(Domain.WithholdingTaxRates.WithholdingTaxRate);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
