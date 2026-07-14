namespace FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

public sealed record JournalEntryLineDto(Guid Id, Guid AccountId, decimal Debit, decimal Credit, string? Description);

public sealed record JournalEntryDto(
    Guid Id,
    string? Reference,
    string Status,
    DateTimeOffset EntryDate,
    decimal TotalDebit,
    decimal TotalCredit,
    IReadOnlyList<JournalEntryLineDto> Lines);
