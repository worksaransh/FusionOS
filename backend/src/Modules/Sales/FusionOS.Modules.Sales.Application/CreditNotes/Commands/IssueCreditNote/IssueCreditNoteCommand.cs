using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;

namespace FusionOS.Modules.Sales.Application.CreditNotes.Commands.IssueCreditNote;

public sealed record IssueCreditNoteCommand(Guid CompanyId, Guid CreditNoteId)
    : ICommand<CreditNoteDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.credit-note.issue" };
    public string EntityType => nameof(Domain.CreditNotes.CreditNote);
    public Guid EntityId => CreditNoteId;
    public string Action => "Issued";
}
