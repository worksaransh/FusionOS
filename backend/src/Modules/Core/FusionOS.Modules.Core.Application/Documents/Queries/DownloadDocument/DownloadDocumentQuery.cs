using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Documents.Contracts;

namespace FusionOS.Modules.Core.Application.Documents.Queries.DownloadDocument;

/// <summary>Fetches one document's actual bytes for streaming back to the caller — the only query in this feature that touches Document.Content.</summary>
public sealed record DownloadDocumentQuery(Guid CompanyId, Guid Id) : IQuery<DocumentContentDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.document.read" };
}
