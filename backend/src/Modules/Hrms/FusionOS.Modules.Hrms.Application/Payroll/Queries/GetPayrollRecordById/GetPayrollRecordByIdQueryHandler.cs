using FusionOS.Modules.Hrms.Application.Payroll.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Payroll.Queries.GetPayrollRecordById;

public sealed class GetPayrollRecordByIdQueryHandler : IRequestHandler<GetPayrollRecordByIdQuery, PayrollRecordDto>
{
    private readonly IPayrollRecordRepository _repository;

    public GetPayrollRecordByIdQueryHandler(IPayrollRecordRepository repository) => _repository = repository;

    public async Task<PayrollRecordDto> Handle(GetPayrollRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CompanyId, request.PayrollRecordId, cancellationToken)
            ?? throw new KeyNotFoundException($"Payroll record '{request.PayrollRecordId}' was not found.");

        return PayrollRecordMapper.ToDto(record);
    }
}
