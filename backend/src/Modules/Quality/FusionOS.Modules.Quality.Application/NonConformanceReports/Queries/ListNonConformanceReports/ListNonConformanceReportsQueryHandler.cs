using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Queries.ListNonConformanceReports;

public sealed class ListNonConformanceReportsQueryHandler : IRequestHandler<ListNonConformanceReportsQuery, PagedResult<NonConformanceReportDto>>
{
    private readonly INonConformanceReportRepository _repository;

    public ListNonConformanceReportsQueryHandler(INonConformanceReportRepository repository) => _repository = repository;

    public async Task<PagedResult<NonConformanceReportDto>> Handle(ListNonConformanceReportsQuery request, CancellationToken cancellationToken)
    {
        var reports = await _repository.ListAsync(request.CompanyId, request.InspectionId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.InspectionId, cancellationToken);

        var dtos = reports.Select(NonConformanceReportMapper.ToDto).ToList();

        return new PagedResult<NonConformanceReportDto>(dtos, request.Page, request.PageSize, total);
    }
}
