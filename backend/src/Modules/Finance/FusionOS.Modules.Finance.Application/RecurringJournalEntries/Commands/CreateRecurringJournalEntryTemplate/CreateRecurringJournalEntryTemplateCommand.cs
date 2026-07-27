using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.CreateRecurringJournalEntryTemplate;

public sealed record CreateRecurringJournalEntryTemplateCommand(
    Guid CompanyId,
    string Reference,
    RecurringJournalEntryFrequency Frequency,
    DateTimeOffset NextDueDate,
    IReadOnlyList<RecurringJournalEntryTemplateLineInput> Lines)
    : ICommand<RecurringJournalEntryTemplateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.recurring-journal-entry.create" };
    public string EntityType => nameof(Domain.RecurringJournalEntries.RecurringJournalEntryTemplate);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
