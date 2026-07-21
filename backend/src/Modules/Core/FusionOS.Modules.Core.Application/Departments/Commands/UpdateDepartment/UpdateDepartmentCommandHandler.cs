using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Departments.Commands.CreateDepartment;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Departments.Commands.UpdateDepartment;

public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDepartmentCommandHandler(IDepartmentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _repository.GetByIdAsync(request.CompanyId, request.DepartmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Department '{request.DepartmentId}' was not found.");

        department.UpdateDetails(request.Name, request.BranchId, request.ParentDepartmentId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateDepartmentCommandHandler.MapToDto(department);
    }
}
