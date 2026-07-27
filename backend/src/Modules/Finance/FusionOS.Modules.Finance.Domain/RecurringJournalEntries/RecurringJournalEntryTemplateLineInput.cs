namespace FusionOS.Modules.Finance.Domain.RecurringJournalEntries;

/// <summary>Input shape for a single recurring journal entry template line — mirrors JournalEntryLineInput's exact shape (minus Description, which a generated JournalEntry line derives from the template's Reference instead). Exactly one of Debit/Credit must be greater than zero. CostCenterId is optional; when supplied it is validated for existence by the command handler.</summary>
public sealed record RecurringJournalEntryTemplateLineInput(Guid AccountId, decimal Debit, decimal Credit, Guid? CostCenterId = null);
