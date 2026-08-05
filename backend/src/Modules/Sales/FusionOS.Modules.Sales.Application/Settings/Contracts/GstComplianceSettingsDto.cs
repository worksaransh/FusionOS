namespace FusionOS.Modules.Sales.Application.Settings.Contracts;

public sealed record GstComplianceSettingsDto(Guid CompanyId, bool EInvoicingRequired);
