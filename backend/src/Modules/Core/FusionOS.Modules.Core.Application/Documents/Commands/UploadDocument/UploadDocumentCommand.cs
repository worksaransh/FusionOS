using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Documents.Contracts;

namespace FusionOS.Modules.Core.Application.Documents.Commands.UploadDocument;

/// <summary>
/// Uploads a file against some (EntityType, EntityId) pair the caller owns —
/// UploadedByUserId is always the authenticated caller (never client-supplied),
/// same reasoning as CreateApprovalRequestCommand.RequestedBy. EntityType/EntityId
/// double as the IAuditableCommand identity too, same precedent.
/// </summary>
public sealed record UploadDocumentCommand(Guid CompanyId, string EntityType, Guid EntityId, string FileName, string? ContentType, byte[] Content)
    : ICommand<DocumentDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.document.create" };
    public string Action => "Uploaded";
}
