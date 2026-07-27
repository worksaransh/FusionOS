namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;

public sealed record RecurringJournalEntryTemplateLineDto(Guid Id, Guid AccountId, decimal Debit, decimal Credit, Guid? CostCenterId);

public sealed record RecurringJournalEntryTemplateDto(
    Guid Id,
    string Reference,
    string Frequency,
    DateTimeOffset NextDueDate,
    bool IsActive,
    decimal TotalDebit,
    decimal TotalCredit,
    IReadOnlyList<RecurringJournalEntryTemplateLineDto> Lines);

/// <summary>Single place that turns a RecurringJournalEntryTemplate aggregate into its DTO, shared by every handler that returns one.</summary>
public static class RecurringJournalEntryTemplateMapper
{
    public static RecurringJournalEntryTemplateDto ToDto(Domain.RecurringJournalEntries.RecurringJournalEntryTemplate template) => new(
        template.Id,
        template.Reference,
        template.Frequency.ToString(),
        template.NextDueDate,
        template.IsActive,
        template.TotalDebit,
        template.TotalCredit,
        template.Lines.Select(l => new RecurringJournalEntryTemplateLineDto(l.Id, l.AccountId, l.Debit, l.Credit, l.CostCenterId)).ToList());
}
