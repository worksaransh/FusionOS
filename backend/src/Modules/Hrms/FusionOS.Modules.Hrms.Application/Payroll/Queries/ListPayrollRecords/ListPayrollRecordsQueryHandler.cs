using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Payroll.Queries.ListPayrollRecords;

public sealed class ListPayrollRecordsQueryHandler : IRequestHandler<ListPayrollRecordsQuery, PagedResult<PayrollRecordDto>>
{
    private readonly IPayrollRecordRepository _repository;

    public ListPayrollRecordsQueryHandler(IPayrollRecordRepository repository) => _repository = repository;

    public async Task<PagedResult<PayrollRecordDto>> Handle(ListPayrollRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.ListAsync(request.CompanyId, request.EmployeeId, request.PeriodMonth, request.PeriodYear, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.EmployeeId, request.PeriodMonth, request.PeriodYear, cancellationToken);

        var dtos = records.Select(PayrollRecordMapper.ToDto).ToList();

        return new PagedResult<PayrollRecordDto>(dtos, request.Page, request.PageSize, total);
    }
}
