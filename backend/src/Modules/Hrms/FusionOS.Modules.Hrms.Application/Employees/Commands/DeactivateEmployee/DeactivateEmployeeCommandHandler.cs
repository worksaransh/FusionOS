using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.DeactivateEmployee;

public sealed class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand, EmployeeDto>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateEmployeeCommandHandler(IEmployeeRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeDto> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(request.CompanyId, request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee '{request.EmployeeId}' was not found.");

        employee.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeMapper.ToDto(employee);
    }
}
