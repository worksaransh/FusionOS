using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Documents.Commands.DeleteDocument;

/// <summary>
/// Soft-deletes a document (Document.MarkDeleted, inherited from
/// TenantAggregateRoot/ISoftDeletable — never a hard row delete, same
/// no-hard-delete convention as every other entity in this codebase; see
/// DeactivateCustomerCommand's doc comment). The global soft-delete query
/// filter in BaseDbContext then hides it from every read, including the list
/// and download endpoints.
/// </summary>
public sealed record DeleteDocumentCommand(Guid CompanyId, Guid Id) : ICommand, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.document.delete" };
    public string EntityType => nameof(Domain.Documents.Document);
    public Guid EntityId => Id;
    public string Action => "Deleted";
}
