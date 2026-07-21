using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Documents.Contracts;

namespace FusionOS.Modules.Core.Application.Documents.Queries.ListDocumentsByEntity;

/// <summary>Every document attached to a given (EntityType, EntityId) pair — the AttachmentsPanel's "list of attached files" query.</summary>
public sealed record ListDocumentsByEntityQuery(Guid CompanyId, string EntityType, Guid EntityId, int Page, int PageSize)
    : IQuery<PagedResult<DocumentDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.document.read" };
}
