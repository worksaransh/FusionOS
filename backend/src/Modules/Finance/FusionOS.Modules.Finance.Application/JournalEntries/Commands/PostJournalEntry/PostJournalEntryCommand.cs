using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Commands.PostJournalEntry;

public sealed record PostJournalEntryCommand(Guid CompanyId, Guid JournalEntryId)
    : ICommand<JournalEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.journal-entry.post" };
    public string EntityType => nameof(Domain.JournalEntries.JournalEntry);
    public Guid EntityId => JournalEntryId;
    public string Action => "Posted";
}
