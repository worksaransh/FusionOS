using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Payroll.Commands.CreatePayrollDraft;

/// <summary>Validates the Employee exists and that no payroll record already exists for this employee/period before creating the draft — same handler-level existence-check split CreateLeaveRequestCommandHandler uses for its own EmployeeId.</summary>
public sealed class CreatePayrollDraftCommandHandler : IRequestHandler<CreatePayrollDraftCommand, PayrollRecordDto>
{
    private readonly IPayrollRecordRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePayrollDraftCommandHandler(
        IPayrollRecordRepository repository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollRecordDto> Handle(CreatePayrollDraftCommand request, CancellationToken cancellationToken)
    {
        if (!await _employeeRepository.ExistsAsync(request.CompanyId, request.EmployeeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.EmployeeId), $"Employee '{request.EmployeeId}' does not exist for this company."),
            });
        }

        if (await _repository.ExistsForPeriodAsync(request.CompanyId, request.EmployeeId, request.PeriodMonth, request.PeriodYear, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.PeriodMonth), $"A payroll record for employee '{request.EmployeeId}' already exists for {request.PeriodMonth}/{request.PeriodYear}."),
            });
        }

        var record = Domain.Payroll.PayrollRecord.CreateDraft(
            request.CompanyId, request.EmployeeId, request.PeriodMonth, request.PeriodYear, request.BaseSalary);

        await _repository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PayrollRecordMapper.ToDto(record);
    }
}
