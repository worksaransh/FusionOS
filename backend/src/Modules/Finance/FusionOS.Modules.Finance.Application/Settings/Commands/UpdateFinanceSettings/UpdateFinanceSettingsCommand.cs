using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Settings.Contracts;

namespace FusionOS.Modules.Finance.Application.Settings.Commands.UpdateFinanceSettings;

public sealed record UpdateFinanceSettingsCommand(
    Guid CompanyId,
    Guid? DefaultArAccountId,
    Guid? DefaultSalesRevenueAccountId,
    Guid? DefaultApAccountId,
    Guid? DefaultPurchaseExpenseAccountId,
    Guid? DefaultDisposalProceedsAccountId = null,
    Guid? DefaultGainLossOnDisposalAccountId = null,
    Guid? DefaultSalaryExpenseAccountId = null,
    Guid? DefaultSalaryPayableAccountId = null)
    : ICommand<FinanceSettingsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.settings.update" };
    public string EntityType => nameof(Domain.Settings.FinanceSettings);
    public Guid EntityId { get; init; }
    public string Action => "Updated";
}
