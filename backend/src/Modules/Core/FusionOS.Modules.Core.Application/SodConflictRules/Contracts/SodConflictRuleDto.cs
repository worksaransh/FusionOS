namespace FusionOS.Modules.Core.Application.SodConflictRules.Contracts;

public sealed record SodConflictRuleDto(Guid Id, string ConflictingPermissionA, string ConflictingPermissionB, string? Description, bool IsActive);
