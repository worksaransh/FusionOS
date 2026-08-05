using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Settings.Contracts;

namespace FusionOS.Modules.Sales.Application.Settings.Queries.GetGstComplianceSettings;

public sealed record GetGstComplianceSettingsQuery(Guid CompanyId) : IQuery<GstComplianceSettingsDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.gst-compliance-settings.read" };
}
