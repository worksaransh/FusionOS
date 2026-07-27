using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Queries.ListAccountingPeriods;

public sealed class ListAccountingPeriodsQueryHandler : IRequestHandler<ListAccountingPeriodsQuery, PagedResult<AccountingPeriodDto>>
{
    private readonly IAccountingPeriodRepository _repository;

    public ListAccountingPeriodsQueryHandler(IAccountingPeriodRepository repository) => _repository = repository;

    public async Task<PagedResult<AccountingPeriodDto>> Handle(ListAccountingPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = periods.Select(AccountingPeriodMapper.ToDto).ToList();

        return new PagedResult<AccountingPeriodDto>(dtos, request.Page, request.PageSize, total);
    }
}
