using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Employees.Queries.ListEmployees;

public sealed class ListEmployeesQueryHandler : IRequestHandler<ListEmployeesQuery, PagedResult<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;

    public ListEmployeesQueryHandler(IEmployeeRepository repository) => _repository = repository;

    public async Task<PagedResult<EmployeeDto>> Handle(ListEmployeesQuery request, CancellationToken cancellationToken)
    {
        var employees = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = employees.Select(EmployeeMapper.ToDto).ToList();

        return new PagedResult<EmployeeDto>(dtos, request.Page, request.PageSize, total);
    }
}
