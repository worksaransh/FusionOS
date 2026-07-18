using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.ExchangeRates.Commands.CreateExchangeRate;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Commands.DeactivateExchangeRate;

public sealed class DeactivateExchangeRateCommandHandler : IRequestHandler<DeactivateExchangeRateCommand, ExchangeRateDto>
{
    private readonly IExchangeRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateExchangeRateCommandHandler(IExchangeRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ExchangeRateDto> Handle(DeactivateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _repository.GetByIdAsync(request.CompanyId, request.ExchangeRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Exchange rate '{request.ExchangeRateId}' was not found.");

        exchangeRate.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateExchangeRateCommandHandler.MapToDto(exchangeRate);
    }
}
