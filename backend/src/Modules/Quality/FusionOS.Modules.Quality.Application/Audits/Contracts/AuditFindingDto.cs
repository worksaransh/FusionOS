namespace FusionOS.Modules.Quality.Application.Audits.Contracts;

public sealed record AuditFindingDto(Guid Id, string Description, string Severity, bool IsResolved);
