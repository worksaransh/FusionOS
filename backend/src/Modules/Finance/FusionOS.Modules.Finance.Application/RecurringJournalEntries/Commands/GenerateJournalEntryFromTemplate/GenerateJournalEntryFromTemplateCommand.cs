using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.GenerateJournalEntryFromTemplate;

/// <summary>
/// Turns one active RecurringJournalEntryTemplate's current lines into a real, persisted
/// JournalEntry, then advances the template's NextDueDate by one Frequency unit — see
/// GenerateJournalEntryFromTemplateCommandHandler's own doc comment for exactly how it reuses
/// CreateJournalEntryCommandHandler's validation (account/cost-center existence, the
/// AccountingPeriod closed-period guard, and the balance check) instead of duplicating it.
/// </summary>
public sealed record GenerateJournalEntryFromTemplateCommand(Guid CompanyId, Guid RecurringJournalEntryTemplateId)
    : ICommand<JournalEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.recurring-journal-entry.generate" };
    public string EntityType => nameof(Domain.RecurringJournalEntries.RecurringJournalEntryTemplate);
    public Guid EntityId => RecurringJournalEntryTemplateId;
    public string Action => "Generated";
}
