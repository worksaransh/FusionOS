using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;
using FusionOS.Modules.Ai.Domain.OcrJobs;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Queries.ListOcrJobs;

/// <summary>Status is optional — omitted, this lists every OCR job for the company; supplied, it scopes to (e.g.) the Failed inbox awaiting attention.</summary>
public sealed record ListOcrJobsQuery(Guid CompanyId, OcrJobStatus? Status = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<OcrJobDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.ocr.read" };
}
