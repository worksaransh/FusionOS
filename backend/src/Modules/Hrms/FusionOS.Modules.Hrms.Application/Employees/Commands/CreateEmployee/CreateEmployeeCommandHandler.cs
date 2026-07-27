using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.CreateEmployee;

public sealed class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(IEmployeeRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // ManagerId is a same-module reference — validated here, same convention as
        // CreateLeaveRequestCommandHandler validating its own EmployeeId. DepartmentId is an
        // opaque cross-module reference into Core and is deliberately never validated (see
        // Employee.cs's class doc comment).
        if (request.ManagerId.HasValue && !await _repository.ExistsAsync(request.CompanyId, request.ManagerId.Value, cancellationToken))
            throw new KeyNotFoundException($"Manager '{request.ManagerId}' was not found.");

        var employee = Domain.Employees.Employee.Create(
            request.CompanyId, request.Code, request.FullName, request.Email, request.DepartmentName, request.HireDate,
            request.DepartmentId, request.ManagerId);

        await _repository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}
