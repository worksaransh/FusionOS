namespace FusionOS.Modules.Finance.Domain.JournalEntries;

/// <summary>Input shape for a single journal entry line. Exactly one of Debit/Credit must be greater than zero.
/// Multidimensional tags (CostCenterId, DepartmentId, ProjectId, LocationId) are optional.
/// ForeignCurrencyCode/ForeignAmount are optional and must be supplied together — see
/// JournalEntryLine's doc comment for what they enable (FX revaluation).</summary>
public sealed record JournalEntryLineInput(
    Guid AccountId,
    decimal Debit,
    decimal Credit,
    string? Description,
    Guid? CostCenterId = null,
    Guid? DepartmentId = null,
    Guid? ProjectId = null,
    Guid? LocationId = null,
    string? ForeignCurrencyCode = null,
    decimal? ForeignAmount = null);
