namespace FusionOS.Modules.Core.Application.Documents.Contracts;

/// <summary>
/// Metadata only — deliberately excludes the file bytes (Document.Content).
/// Listing documents against an entity should never pull every attached
/// file's full byte content over the wire; only DocumentContentDto (returned
/// by the single-document download endpoint) carries the actual bytes.
/// </summary>
public sealed record DocumentDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    Guid UploadedByUserId,
    DateTimeOffset UploadedAt);

/// <summary>The actual file bytes for a single document — only ever returned by the download endpoint, never by the list endpoint.</summary>
public sealed record DocumentContentDto(string FileName, string ContentType, byte[] Content);
