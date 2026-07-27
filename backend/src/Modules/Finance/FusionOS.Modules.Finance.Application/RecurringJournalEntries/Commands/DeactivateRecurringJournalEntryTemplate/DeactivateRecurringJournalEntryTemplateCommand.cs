using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.DeactivateRecurringJournalEntryTemplate;

public sealed record DeactivateRecurringJournalEntryTemplateCommand(Guid CompanyId, Guid RecurringJournalEntryTemplateId)
    : ICommand<RecurringJournalEntryTemplateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.recurring-journal-entry.deactivate" };
    public string EntityType => nameof(Domain.RecurringJournalEntries.RecurringJournalEntryTemplate);
    public Guid EntityId => RecurringJournalEntryTemplateId;
    public string Action => "Deactivated";
}
