using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using MediatR;

namespace FusionOS.Modules.Core.Application.Departments.Commands.CreateDepartment;

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepartmentCommandHandler(IDepartmentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = Department.Create(request.CompanyId, request.BranchId, request.Name, request.Code, request.ParentDepartmentId);

        await _repository.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(department);
    }

    internal static DepartmentDto MapToDto(Department department) => new(
        department.Id, department.CompanyId, department.BranchId, department.Name, department.Code, department.ParentDepartmentId, department.IsActive, department.CreatedAt);
}
