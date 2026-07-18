using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Commissions.Commands.SetCommissionRate;
using FusionOS.Modules.Sales.Application.Commissions.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Commissions.Queries.ListCommissionRates;

public sealed class ListCommissionRatesQueryHandler : IRequestHandler<ListCommissionRatesQuery, PagedResult<SalesCommissionRateDto>>
{
    private readonly ISalesCommissionRateRepository _repository;

    public ListCommissionRatesQueryHandler(ISalesCommissionRateRepository repository) => _repository = repository;

    public async Task<PagedResult<SalesCommissionRateDto>> Handle(ListCommissionRatesQuery request, CancellationToken cancellationToken)
    {
        var rates = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = rates.Select(SetCommissionRateCommandHandler.MapToDto).ToList();

        return new PagedResult<SalesCommissionRateDto>(dtos, request.Page, request.PageSize, total);
    }
}
