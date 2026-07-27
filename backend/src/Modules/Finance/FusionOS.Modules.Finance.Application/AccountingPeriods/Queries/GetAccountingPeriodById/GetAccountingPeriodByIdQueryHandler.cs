using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Queries.GetAccountingPeriodById;

public sealed class GetAccountingPeriodByIdQueryHandler : IRequestHandler<GetAccountingPeriodByIdQuery, AccountingPeriodDto>
{
    private readonly IAccountingPeriodRepository _repository;

    public GetAccountingPeriodByIdQueryHandler(IAccountingPeriodRepository repository) => _repository = repository;

    public async Task<AccountingPeriodDto> Handle(GetAccountingPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        var period = await _repository.GetByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new KeyNotFoundException($"Accounting period '{request.AccountingPeriodId}' was not found.");

        return AccountingPeriodMapper.ToDto(period);
    }
}
