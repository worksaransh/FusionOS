using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Settings.Contracts;

namespace FusionOS.Modules.Sales.Application.Settings.Commands.UpdateGstComplianceSettings;

public sealed record UpdateGstComplianceSettingsCommand(Guid CompanyId, bool EInvoicingRequired)
    : ICommand<GstComplianceSettingsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.gst-compliance-settings.update" };
    public string EntityType => nameof(Domain.Settings.GstComplianceSettings);
    public Guid EntityId { get; init; }
    public string Action => "Updated";
}
