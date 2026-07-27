using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.UpdateRecurringJournalEntryTemplate;

public sealed record UpdateRecurringJournalEntryTemplateCommand(
    Guid CompanyId,
    Guid RecurringJournalEntryTemplateId,
    string Reference,
    RecurringJournalEntryFrequency Frequency,
    DateTimeOffset NextDueDate,
    IReadOnlyList<RecurringJournalEntryTemplateLineInput> Lines)
    : ICommand<RecurringJournalEntryTemplateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.recurring-journal-entry.update" };
    public string EntityType => nameof(Domain.RecurringJournalEntries.RecurringJournalEntryTemplate);
    public Guid EntityId => RecurringJournalEntryTemplateId;
    public string Action => "Updated";
}
