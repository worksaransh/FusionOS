using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Commands.SubmitOcrJob;

/// <summary>
/// Submits an already-uploaded Core Document (see Core's File Management,
/// `FusionOS.Modules.Core.Domain.Documents.Document`) for OCR extraction. DocumentId is an
/// opaque cross-module reference — never existence-validated here, same convention as
/// every other cross-module reference in this codebase (see OcrJob's class doc comment).
/// </summary>
public sealed record SubmitOcrJobCommand(Guid CompanyId, Guid DocumentId)
    : ICommand<OcrJobDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "ai.ocr.submit" };
    public string EntityType => nameof(Domain.OcrJobs.OcrJob);
    public Guid EntityId { get; init; }
    public string Action => "Submitted for OCR";
}
