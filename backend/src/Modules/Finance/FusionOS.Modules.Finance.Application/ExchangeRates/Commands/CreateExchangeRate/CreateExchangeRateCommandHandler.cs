using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using MediatR;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;

public sealed class CreateExchangeRateCommandHandler : IRequestHandler<CreateExchangeRateCommand, ExchangeRateDto>
{
    private readonly IExchangeRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateExchangeRateCommandHandler(IExchangeRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ExchangeRateDto> Handle(CreateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.RateExistsAsync(request.CompanyId, request.FromCurrencyCode, request.ToCurrencyCode, request.EffectiveDate, null, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.EffectiveDate), $"A rate for {request.FromCurrencyCode}->{request.ToCurrencyCode} already exists on {request.EffectiveDate:yyyy-MM-dd} — use UpdateExchangeRateCommand to correct it instead."),
            });
        }

        var exchangeRate = ExchangeRate.Create(request.CompanyId, request.FromCurrencyCode, request.ToCurrencyCode, request.Rate, request.EffectiveDate);

        await _repository.AddAsync(exchangeRate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(exchangeRate);
    }

    internal static ExchangeRateDto MapToDto(ExchangeRate exchangeRate) => new(
        exchangeRate.Id, exchangeRate.FromCurrencyCode, exchangeRate.ToCurrencyCode, exchangeRate.Rate,
        exchangeRate.EffectiveDate, exchangeRate.IsActive, exchangeRate.CreatedAt);
}
