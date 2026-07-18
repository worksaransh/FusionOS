using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Queries.ListExchangeRates;

public sealed class ListExchangeRatesQueryHandler : IRequestHandler<ListExchangeRatesQuery, PagedResult<ExchangeRateDto>>
{
    private readonly IExchangeRateRepository _repository;

    public ListExchangeRatesQueryHandler(IExchangeRateRepository repository) => _repository = repository;

    public async Task<PagedResult<ExchangeRateDto>> Handle(ListExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var exchangeRates = await _repository.ListAsync(request.CompanyId, request.FromCurrencyCode, request.ToCurrencyCode, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.FromCurrencyCode, request.ToCurrencyCode, cancellationToken);

        var dtos = exchangeRates.Select(CreateExchangeRateCommandHandler.MapToDto).ToList();

        return new PagedResult<ExchangeRateDto>(dtos, request.Page, request.PageSize, total);
    }
}
