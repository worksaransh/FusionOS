namespace FusionOS.Modules.Ai.Application.OcrJobs;

/// <summary>
/// The only <see cref="IOcrProvider"/> implementation registered today (see
/// <c>AiModule.RegisterServices</c>). It always fails, with a message that names exactly
/// what's missing and how to wire in a real one — this is deliberate, honest behavior:
/// the job-tracking pipeline (submit → Processing → Failed, durably recorded, queryable)
/// is genuinely functional end-to-end without pretending to extract text no engine here
/// can actually extract. Swap this registration for a real provider once one is stood up;
/// nothing else in the OCR pipeline needs to change.
/// </summary>
public sealed class UnavailableOcrProvider : IOcrProvider
{
    public Task<OcrExtractionResult> ExtractAsync(Guid documentId, CancellationToken cancellationToken) =>
        Task.FromResult(new OcrExtractionResult(
            Succeeded: false,
            ExtractedFieldsJson: null,
            FailureReason: "No OCR provider is configured. Wire a real provider (e.g. Azure AI Document Intelligence, " +
                "Google Cloud Vision, AWS Textract, or a self-hosted Tesseract/vision-model service) by implementing " +
                "IOcrProvider and registering it in AiModule.cs — this stub exists so the job-tracking pipeline is " +
                "genuinely functional end-to-end (submit → fail with a clear, honest reason) without pretending to " +
                "extract text it cannot actually extract."));
}
