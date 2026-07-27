using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.RecurringJournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Queries.ListRecurringJournalEntryTemplates;

/// <summary>DueNow, when true, restricts the list to active templates whose NextDueDate has passed — "templates ready to generate". Omitted (default false), it lists every template for the company regardless of due date or active status.</summary>
public sealed record ListRecurringJournalEntryTemplatesQuery(Guid CompanyId, bool DueNow = false, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<RecurringJournalEntryTemplateDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.recurring-journal-entry.read" };
}
