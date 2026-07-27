using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Queries.ListOcrJobs;

public sealed class ListOcrJobsQueryHandler : IRequestHandler<ListOcrJobsQuery, PagedResult<OcrJobDto>>
{
    private readonly IOcrJobRepository _repository;

    public ListOcrJobsQueryHandler(IOcrJobRepository repository) => _repository = repository;

    public async Task<PagedResult<OcrJobDto>> Handle(ListOcrJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _repository.ListAsync(request.CompanyId, request.Status, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Status, cancellationToken);

        var dtos = jobs.Select(OcrJobMapper.ToDto).ToList();

        return new PagedResult<OcrJobDto>(dtos, request.Page, request.PageSize, total);
    }
}
