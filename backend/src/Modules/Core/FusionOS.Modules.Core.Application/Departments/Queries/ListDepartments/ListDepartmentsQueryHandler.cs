using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Departments.Commands.CreateDepartment;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Departments.Queries.ListDepartments;

public sealed class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, PagedResult<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository;

    public ListDepartmentsQueryHandler(IDepartmentRepository repository) => _repository = repository;

    public async Task<PagedResult<DepartmentDto>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = departments.Select(CreateDepartmentCommandHandler.MapToDto).ToList();

        return new PagedResult<DepartmentDto>(dtos, request.Page, request.PageSize, total);
    }
}
