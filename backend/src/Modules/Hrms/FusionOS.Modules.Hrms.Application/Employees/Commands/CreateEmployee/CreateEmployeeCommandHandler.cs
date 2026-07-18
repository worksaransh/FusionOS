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
        var employee = Domain.Employees.Employee.Create(request.CompanyId, request.Code, request.FullName, request.Email, request.DepartmentName, request.HireDate);

        await _repository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}
