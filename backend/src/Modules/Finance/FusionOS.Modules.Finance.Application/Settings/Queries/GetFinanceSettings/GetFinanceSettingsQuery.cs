using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Settings.Contracts;

namespace FusionOS.Modules.Finance.Application.Settings.Queries.GetFinanceSettings;

public sealed record GetFinanceSettingsQuery(Guid CompanyId) : IQuery<FinanceSettingsDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.settings.read" };
}
