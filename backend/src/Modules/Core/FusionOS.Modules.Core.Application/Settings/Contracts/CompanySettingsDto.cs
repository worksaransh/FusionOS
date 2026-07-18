namespace FusionOS.Modules.Core.Application.Settings.Contracts;

public sealed record CompanySettingsDto(Guid CompanyId, string DefaultCurrency, int DefaultPageSize, string? DisplayName, string? LogoUrl);
