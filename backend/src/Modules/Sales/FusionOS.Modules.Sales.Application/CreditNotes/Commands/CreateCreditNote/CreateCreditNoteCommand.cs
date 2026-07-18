using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using FusionOS.Modules.Sales.Domain.CreditNotes;

namespace FusionOS.Modules.Sales.Application.CreditNotes.Commands.CreateCreditNote;

public sealed record CreateCreditNoteCommand(Guid CompanyId, Guid InvoiceId, Guid CustomerId, string Reason, IReadOnlyList<CreditNoteLineInput> Lines)
    : ICommand<CreditNoteDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.credit-note.create" };
    public string EntityType => nameof(Domain.CreditNotes.CreditNote);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
