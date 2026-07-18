namespace FusionOS.Modules.Finance.Domain.JournalEntries;

/// <summary>Input shape for a single journal entry line. Exactly one of Debit/Credit must be greater than zero.
/// CostCenterId is optional; when supplied it is validated for existence by the command handler.</summary>
public sealed record JournalEntryLineInput(Guid AccountId, decimal Debit, decimal Credit, string? Description, Guid? CostCenterId = null);
