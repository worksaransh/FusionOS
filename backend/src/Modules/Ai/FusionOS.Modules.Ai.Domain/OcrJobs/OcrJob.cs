using FusionOS.SharedKernel;
using FusionOS.Modules.Ai.Domain.OcrJobs.Events;

namespace FusionOS.Modules.Ai.Domain.OcrJobs;

/// <summary>
/// Phase 8 — AI &amp; Integrations. `docs/blueprint/12_AI_PLATFORM.md` §4 describes OCR/invoice
/// processing as: a document ingested via Core's File Management, sent to a Python
/// vision-model service for extraction, and the resulting fields proposed back into
/// Procurement/Finance as a draft record requiring human confirmation before posting
/// ("OCR output is a draft, never auto-posted to the ledger"). This aggregate is the
/// durable, audit-tracked job record for that pipeline: Pending → Processing →
/// Completed/Failed, exactly the state machine a real async extraction call needs
/// regardless of which OCR backend eventually implements <c>IOcrProvider</c>.
///
/// There is no real OCR engine, vision-model API, or Python service reachable in this
/// environment (`02_TECH_STACK.md` §9's Python ML service is a separate, unbuilt
/// workstream — see Recommendation's own class doc comment for the same restraint
/// applied to forecasting/ML). Building a fake text-extraction result here would look
/// done in a grep and not be. What IS real and complete: the job entity, its state
/// machine, the Core Document cross-reference, and the application layer's
/// <c>IOcrProvider</c> seam a real provider plugs into later (see that interface's own
/// doc comment for the concrete extension point).
///
/// <see cref="DocumentId"/> is an opaque cross-module reference into Core's
/// <c>Document</c> aggregate (`FusionOS.Modules.Core.Domain.Documents.Document`) —
/// never existence-validated here, same no-cross-module-FK convention as Quality's
/// Inspection.ReferenceId and every other cross-module reference in this codebase.
/// The caller is responsible for having already uploaded the document via Core's
/// File Management before submitting it for OCR.
///
/// <see cref="ExtractedFieldsJson"/> is stored as raw JSON text, not a strongly-typed
/// column, because extracted fields genuinely vary by document type (a vendor invoice
/// yields different fields than a packing slip) — same "don't invent a schema you don't
/// have real data to justify" reasoning Document's own class doc comment applies to
/// file storage.
///
/// <see cref="DraftReferenceType"/>/<see cref="DraftReferenceId"/> are set once (and only
/// once) a draft record is created from this job's extraction — an opaque polymorphic
/// reference into whatever module ends up owning that draft (Procurement's vendor
/// invoice matching, or Finance's AP entry), same convention as Quality's
/// Inspection.ReferenceId.
/// </summary>
public sealed class OcrJob : TenantAggregateRoot
{
    public Guid DocumentId { get; private set; }
    public OcrJobStatus Status { get; private set; }
    public string? ExtractedFieldsJson { get; private set; }
    public string? FailureReason { get; private set; }
    public string? DraftReferenceType { get; private set; }
    public Guid? DraftReferenceId { get; private set; }

    private OcrJob() { }

    public static OcrJob Submit(Guid companyId, Guid documentId)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("A document id (from Core's File Management) is required.", nameof(documentId));

        var job = new OcrJob
        {
            CompanyId = companyId,
            DocumentId = documentId,
            Status = OcrJobStatus.Pending,
        };

        job.Raise(new OcrJobSubmitted(job.Id, companyId, documentId));
        return job;
    }

    /// <summary>Requires the job to still be Pending — same "one clear starting state" discipline as MaintenanceRequest.Start/LeaveRequest.Approve.</summary>
    public void StartProcessing()
    {
        if (Status != OcrJobStatus.Pending)
            throw new InvalidOperationException($"Only a Pending OCR job can start processing (current status: {Status}).");

        Status = OcrJobStatus.Processing;
    }

    public void CompleteWithExtraction(string extractedFieldsJson)
    {
        if (Status != OcrJobStatus.Processing)
            throw new InvalidOperationException($"Only a Processing OCR job can be completed (current status: {Status}).");
        if (string.IsNullOrWhiteSpace(extractedFieldsJson))
            throw new ArgumentException("Extracted fields JSON is required to complete an OCR job.", nameof(extractedFieldsJson));

        ExtractedFieldsJson = extractedFieldsJson;
        Status = OcrJobStatus.Completed;
        Raise(new OcrJobCompleted(Id, CompanyId, DocumentId));
    }

    public void Fail(string reason)
    {
        if (Status != OcrJobStatus.Processing)
            throw new InvalidOperationException($"Only a Processing OCR job can fail (current status: {Status}).");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A failure reason is required.", nameof(reason));

        FailureReason = reason.Trim();
        Status = OcrJobStatus.Failed;
        Raise(new OcrJobFailed(Id, CompanyId, DocumentId, FailureReason));
    }

    /// <summary>
    /// Records that a draft record (a Procurement vendor-invoice match or a Finance AP
    /// entry, per §4) was created from this job's extraction. Only reachable once
    /// Completed — an OCR job that failed or is still in flight has no extraction to
    /// build a draft from. See <c>CreateDraftFromOcrExtractionCommandHandler</c> for why,
    /// with only <see cref="Application"/>'s stub <c>IOcrProvider</c> wired up today, this
    /// method is exercised by tests but never reachable through the real submit pipeline.
    /// </summary>
    public void LinkDraftRecord(string referenceType, Guid referenceId)
    {
        if (Status != OcrJobStatus.Completed)
            throw new InvalidOperationException($"A draft record can only be linked to a Completed OCR job (current status: {Status}).");
        if (string.IsNullOrWhiteSpace(referenceType))
            throw new ArgumentException("A draft reference type is required.", nameof(referenceType));
        if (referenceId == Guid.Empty)
            throw new ArgumentException("A draft reference id is required.", nameof(referenceId));

        DraftReferenceType = referenceType.Trim();
        DraftReferenceId = referenceId;
    }
}
