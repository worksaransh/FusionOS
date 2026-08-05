namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;

public sealed record IntercompanyEliminationRuleDto(
    Guid Id,
    string ReceivableAccountCode,
    string PayableAccountCode,
    string Description,
    bool IsActive);
