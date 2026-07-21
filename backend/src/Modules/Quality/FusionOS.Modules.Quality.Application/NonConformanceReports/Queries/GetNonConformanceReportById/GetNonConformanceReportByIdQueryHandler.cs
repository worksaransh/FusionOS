using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Queries.GetNonConformanceReportById;

public sealed class GetNonConformanceReportByIdQueryHandler : IRequestHandler<GetNonConformanceReportByIdQuery, NonConformanceReportDto>
{
    private readonly INonConformanceReportRepository _repository;

    public GetNonConformanceReportByIdQueryHandler(INonConformanceReportRepository repository) => _repository = repository;

    public async Task<NonConformanceReportDto> Handle(GetNonConformanceReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdAsync(request.CompanyId, request.NonConformanceReportId, cancellationToken)
            ?? throw new KeyNotFoundException($"Non-conformance report '{request.NonConformanceReportId}' was not found.");

        return NonConformanceReportMapper.ToDto(report);
    }
}
