using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Payroll.Commands.MarkPayrollRecordPaid;

public sealed class MarkPayrollRecordPaidCommandHandler : IRequestHandler<MarkPayrollRecordPaidCommand, PayrollRecordDto>
{
    private readonly IPayrollRecordRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkPayrollRecordPaidCommandHandler(IPayrollRecordRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PayrollRecordDto> Handle(MarkPayrollRecordPaidCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CompanyId, request.PayrollRecordId, cancellationToken)
            ?? throw new KeyNotFoundException($"Payroll record '{request.PayrollRecordId}' was not found.");

        record.MarkPaid(DateTimeOffset.UtcNow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PayrollRecordMapper.ToDto(record);
    }
}
