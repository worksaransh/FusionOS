using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Documents;

/// <summary>
/// Generic, module-agnostic file attachment (net-new subsystem, 2026-07-21) —
/// any record in any module (an Invoice, a PurchaseOrder, an Employee, a
/// MaintenanceRequest, anything) can have files attached to it via this one
/// entity. EntityType/EntityId are opaque references into whatever aggregate
/// this document is attached to — same no-cross-module-FK convention already
/// established by FusionOS.Modules.Core.Domain.Workflow.ApprovalRequest (see
/// its doc comment); this entity doesn't know or care what a "PurchaseOrder"
/// is, it only tracks a file against some (EntityType, EntityId) pair.
///
/// Storage decision: file bytes are stored directly in Postgres (a `bytea`
/// column, see DocumentConfiguration) rather than a filesystem/S3/Azure Blob
/// integration. There is no cloud storage configured anywhere in this repo
/// today (no such section in appsettings.json, no cloud SDK package
/// referenced) — inventing one with no real backend to point at would be
/// dishonest scaffolding, not a working feature. Storing bytes in Postgres is
/// the minimal-but-real choice given the actual infrastructure available.
///
/// This does NOT scale to large files — bytea-in-Postgres bloats table/row
/// size, WAL volume, and backup time, and every read pulls the full blob into
/// application memory. MaxFileSizeBytes (10 MB) is enforced here specifically
/// because of that limitation, not as an arbitrary business rule. The natural
/// follow-up, once a cloud storage account is actually provisioned, is to
/// replace the Content column with a StorageKey/Url pointing at S3 or Azure
/// Blob Storage and stream bytes from there instead — deliberately not done
/// in this pass since there is nothing configured to point at yet.
/// </summary>
public sealed class Document : TenantAggregateRoot
{
    /// <summary>10 MB — see this class's doc comment for why bytea-in-Postgres caps out here.</summary>
    public const int MaxFileSizeBytes = 10 * 1024 * 1024;

    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public long FileSizeBytes { get; private set; }
    public byte[] Content { get; private set; } = Array.Empty<byte>();
    public Guid UploadedByUserId { get; private set; }
    public DateTimeOffset UploadedAt { get; private set; }

    private Document() { }

    public static Document Upload(Guid companyId, string entityType, Guid entityId, string fileName, string? contentType, byte[] content, Guid uploadedByUserId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));
        if (content is null || content.Length == 0)
            throw new ArgumentException("File content must not be empty.", nameof(content));
        if (content.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.", nameof(content));
        if (uploadedByUserId == Guid.Empty)
            throw new ArgumentException("Uploaded-by user id is required.", nameof(uploadedByUserId));

        return new Document
        {
            CompanyId = companyId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            FileName = fileName.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim(),
            FileSizeBytes = content.Length,
            Content = content,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTimeOffset.UtcNow,
        };
    }
}
