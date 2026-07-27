using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.UpdateEmployee;

public sealed class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeCommandHandler(IEmployeeRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(request.CompanyId, request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee '{request.EmployeeId}' was not found.");

        // ManagerId is a same-module reference — validated here, same convention CreateEmployeeCommandHandler
        // uses for its own ManagerId. DepartmentId is an opaque cross-module reference into Core and is
        // deliberately never validated (see Employee.cs's class doc comment).
        if (request.ManagerId.HasValue && !await _repository.ExistsAsync(request.CompanyId, request.ManagerId.Value, cancellationToken))
            throw new KeyNotFoundException($"Manager '{request.ManagerId}' was not found.");

        employee.UpdateDetails(request.FullName, request.Email, request.DepartmentId, request.ManagerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}
