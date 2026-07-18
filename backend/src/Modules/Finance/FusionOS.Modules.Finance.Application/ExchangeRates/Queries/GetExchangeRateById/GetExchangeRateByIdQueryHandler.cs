using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Queries.GetExchangeRateById;

public sealed class GetExchangeRateByIdQueryHandler : IRequestHandler<GetExchangeRateByIdQuery, ExchangeRateDto>
{
    private readonly IExchangeRateRepository _repository;

    public GetExchangeRateByIdQueryHandler(IExchangeRateRepository repository) => _repository = repository;

    public async Task<ExchangeRateDto> Handle(GetExchangeRateByIdQuery request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _repository.GetByIdAsync(request.CompanyId, request.ExchangeRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exchange rate '{request.ExchangeRateId}' was not found.");

        return CreateExchangeRateCommandHandler.MapToDto(exchangeRate);
    }
}
