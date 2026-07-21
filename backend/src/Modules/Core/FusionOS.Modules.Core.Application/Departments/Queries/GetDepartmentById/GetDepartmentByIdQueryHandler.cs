using FusionOS.Modules.Core.Application.Departments.Commands.CreateDepartment;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Departments.Queries.GetDepartmentById;

public sealed class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto>
{
    private readonly IDepartmentRepository _repository;

    public GetDepartmentByIdQueryHandler(IDepartmentRepository repository) => _repository = repository;

    public async Task<DepartmentDto> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _repository.GetByIdAsync(request.CompanyId, request.DepartmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Department '{request.DepartmentId}' was not found.");

        return CreateDepartmentCommandHandler.MapToDto(department);
    }
}
