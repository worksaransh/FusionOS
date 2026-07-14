using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;

namespace FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;

public sealed record CreateJournalEntryCommand(Guid CompanyId, string? Reference, IReadOnlyList<JournalEntryLineInput> Lines)
    : ICommand<JournalEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.journal-entry.create" };
    public string EntityType => nameof(Domain.JournalEntries.JournalEntry);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
