using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Commands.CreateDraftFromOcrExtraction;

/// <summary>
/// Proves the "extracted fields become a draft record" pipeline shape described in
/// `docs/blueprint/12_AI_PLATFORM.md` §4 ("structured data proposed back into Procurement...
/// or Finance... as a draft record requiring human confirmation before posting"). Given a
/// Completed OcrJob, this parses <c>ExtractedFieldsJson</c> and records the bookkeeping link
/// (<see cref="Domain.OcrJobs.OcrJob.LinkDraftRecord"/>) back to wherever a real draft record
/// ends up living (Procurement vendor-invoice matching, or Finance's AP entry).
///
/// It deliberately does NOT construct a real Finance/Procurement draft bill in this pass —
/// doing that would require a real schema for "what fields does an OCR'd invoice produce",
/// which does not exist without a real <see cref="IOcrProvider"/> actually producing data.
/// Constructing one from imagined field names would be exactly the "looks done in a grep,
/// isn't" trap this whole slice is designed to avoid. <see cref="TargetReferenceType"/>/
/// <see cref="TargetReferenceId"/> are supplied by the caller (e.g. after they manually
/// create the real Procurement/Finance draft record through that module's own API) purely
/// so this command can record the cross-reference; wiring the real draft-creation call is
/// the documented next step once a provider exists (see <see cref="IOcrProvider"/>).
///
/// With only <see cref="UnavailableOcrProvider"/> registered today, no OcrJob can ever reach
/// Completed, so this command's happy path is not reachable through the real submit flow —
/// <see cref="CreateDraftFromOcrExtractionCommandHandler"/> throws a
/// <see cref="FusionOS.BuildingBlocks.Application.Exceptions.ValidationException"/> for any
/// job that isn't Completed, which makes that current limitation visible rather than a
/// silent no-op.
/// </summary>
public sealed record CreateDraftFromOcrExtractionCommand(Guid CompanyId, Guid OcrJobId, string TargetReferenceType, Guid TargetReferenceId)
    : ICommand<OcrJobDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "ai.ocr.submit" };
    public string EntityType => nameof(Domain.OcrJobs.OcrJob);
    public Guid EntityId => OcrJobId;
    public string Action => "Linked draft record from OCR extraction";
}
