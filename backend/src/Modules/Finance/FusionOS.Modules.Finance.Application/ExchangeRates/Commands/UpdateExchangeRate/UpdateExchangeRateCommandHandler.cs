using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.UpdateExchangeRate;

public sealed class UpdateExchangeRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, ExchangeRateDto>
{
    private readonly IExchangeRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateExchangeRateCommandHandler(IExchangeRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ExchangeRateDto> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _repository.GetByIdAsync(request.CompanyId, request.ExchangeRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exchange rate '{request.ExchangeRateId}' was not found.");

        // Only re-check the uniqueness tuple if EffectiveDate is actually changing —
        // same "don't trip over the row being updated" excludeId pattern
        // IExchangeRateRepository.RateExistsAsync documents.
        if (request.EffectiveDate != exchangeRate.EffectiveDate &&
            await _repository.RateExistsAsync(request.CompanyId, exchangeRate.FromCurrencyCode, exchangeRate.ToCurrencyCode, request.EffectiveDate, exchangeRate.Id, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.EffectiveDate), $"A rate for {exchangeRate.FromCurrencyCode}->{exchangeRate.ToCurrencyCode} already exists on {request.EffectiveDate:yyyy-MM-dd}."),
            });
        }

        exchangeRate.UpdateRate(request.Rate, request.EffectiveDate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateExchangeRateCommandHandler.MapToDto(exchangeRate);
    }
}
