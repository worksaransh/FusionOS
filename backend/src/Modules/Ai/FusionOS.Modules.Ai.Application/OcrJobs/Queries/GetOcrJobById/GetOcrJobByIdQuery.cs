using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Queries.GetOcrJobById;

public sealed record GetOcrJobByIdQuery(Guid CompanyId, Guid OcrJobId) : IQuery<OcrJobDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.ocr.read" };
}
