using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Employees.Queries.GetEmployeeById;

public sealed class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    private readonly IEmployeeRepository _repository;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository repository) => _repository = repository;

    public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(request.CompanyId, request.EmployeeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Employee '{request.EmployeeId}' was not found.");

        return EmployeeMapper.ToDto(employee);
    }
}
