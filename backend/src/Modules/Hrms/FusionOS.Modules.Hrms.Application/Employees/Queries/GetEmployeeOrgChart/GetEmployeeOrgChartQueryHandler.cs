using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Employees.Queries.GetEmployeeOrgChart;

/// <summary>
/// Single-level "my reports" org-chart view (see EmployeeOrgChartDto's own doc comment for the
/// deliberate scope limit — no recursive reports-of-reports tree). A future
/// GetEmployeeOrgChartTreeQuery could add that by recursing over ListDirectReportsAsync (or a
/// dedicated recursive CTE) once that depth is actually needed.
/// </summary>
public sealed class GetEmployeeOrgChartQueryHandler : IRequestHandler<GetEmployeeOrgChartQuery, EmployeeOrgChartDto>
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeOrgChartQueryHandler(IEmployeeRepository repository) => _repository = repository;

    public async Task<EmployeeOrgChartDto> Handle(GetEmployeeOrgChartQuery request, CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(request.CompanyId, request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee '{request.EmployeeId}' was not found.");

        var directReports = await _repository.ListDirectReportsAsync(request.CompanyId, request.EmployeeId, cancellationToken);

        return new EmployeeOrgChartDto(EmployeeMapper.ToDto(employee), directReports.Select(EmployeeMapper.ToDto).ToList());
    }
}
