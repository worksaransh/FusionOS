using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Queries.GetRecurringJournalEntryTemplateById;

public sealed record GetRecurringJournalEntryTemplateByIdQuery(Guid CompanyId, Guid RecurringJournalEntryTemplateId) : IQuery<RecurringJournalEntryTemplateDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.recurring-journal-entry.read" };
}
