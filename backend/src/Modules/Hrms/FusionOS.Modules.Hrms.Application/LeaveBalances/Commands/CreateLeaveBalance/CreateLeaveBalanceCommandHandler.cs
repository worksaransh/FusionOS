using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.CreateLeaveBalance;

/// <summary>
/// Validates the Employee exists for this company (same handler-level existence-check
/// split CreateLeaveRequestCommandHandler uses for EmployeeId) and that no balance row
/// already exists for this (company, employee, type, year) — the duplicate-code check
/// idiom used elsewhere in this codebase for natural-key uniqueness (e.g.
/// UpdateRoleCommandHandler's RoleNameExistsAsync check).
/// </summary>
public sealed class CreateLeaveBalanceCommandHandler : IRequestHandler<CreateLeaveBalanceCommand, LeaveBalanceDto>
{
    private readonly ILeaveBalanceRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeaveBalanceCommandHandler(ILeaveBalanceRepository repository, IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveBalanceDto> Handle(CreateLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        if (!await _employeeRepository.ExistsAsync(request.CompanyId, request.EmployeeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.EmployeeId), $"Employee '{request.EmployeeId}' does not exist for this company."),
            });
        }

        if (await _repository.ExistsAsync(request.CompanyId, request.EmployeeId, request.LeaveType, request.Year, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Year), $"A leave balance already exists for employee '{request.EmployeeId}', type '{request.LeaveType}', year {request.Year}."),
            });
        }

        var balance = Domain.LeaveBalances.LeaveBalance.Create(
            request.CompanyId, request.EmployeeId, request.LeaveType, request.Year, request.EntitledDays);

        await _repository.AddAsync(balance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeaveBalanceMapper.ToDto(balance);
    }
}
