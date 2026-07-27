namespace FusionOS.Modules.Ai.Application.OcrJobs;

/// <summary>
/// The extension seam `docs/blueprint/12_AI_PLATFORM.md` §4 describes as "OCR/extraction
/// (Python service, likely leveraging a vision-capable model)". FusionOS does not run any
/// such service, vision-model API, or self-hosted OCR engine in this environment — there is
/// no genuine way to extract text from an image/PDF here. Rather than fabricate fake
/// extracted data (which would look done in a grep and quietly corrupt a Procurement/Finance
/// draft downstream), this interface is the real, honest extension point: implement it
/// against a real backend (Azure AI Document Intelligence, Google Cloud Vision, AWS Textract,
/// or a self-hosted Tesseract/vision-model service) and register it in <c>AiModule.cs</c> in
/// place of <see cref="UnavailableOcrProvider"/> once one exists.
///
/// <see cref="SubmitOcrJobCommandHandler"/> calls this synchronously, in the same request as
/// the job's own Pending→Processing→Completed/Failed transition — see that handler's doc
/// comment for why (no background job queue infrastructure exists in this codebase yet to
/// build a real async pipeline on top of).
/// </summary>
public interface IOcrProvider
{
    Task<OcrExtractionResult> ExtractAsync(Guid documentId, CancellationToken cancellationToken);
}

/// <summary>
/// <paramref name="ExtractedFieldsJson"/> is only meaningful when <paramref name="Succeeded"/>
/// is true; <paramref name="FailureReason"/> is only meaningful when it is false. A real
/// provider's ExtractedFieldsJson shape will vary by document type (vendor invoice vs.
/// packing slip) — see OcrJob's own class doc comment for why that's stored as raw JSON text
/// rather than a strongly-typed contract here.
/// </summary>
public sealed record OcrExtractionResult(bool Succeeded, string? ExtractedFieldsJson, string? FailureReason);
