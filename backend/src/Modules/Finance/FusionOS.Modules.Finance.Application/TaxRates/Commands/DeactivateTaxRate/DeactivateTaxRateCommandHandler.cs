using FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.DeactivateTaxRate;

public sealed class DeactivateTaxRateCommandHandler : IRequestHandler<DeactivateTaxRateCommand, TaxRateDto>
{
    private readonly ITaxRateRepository _repository;
    private readonly FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork _unitOfWork;

    public DeactivateTaxRateCommandHandler(ITaxRateRepository repository, FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaxRateDto> Handle(DeactivateTaxRateCommand request, CancellationToken cancellationToken)
    {
        var taxRate = await _repository.GetByIdAsync(request.CompanyId, request.TaxRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tax rate '{request.TaxRateId}' was not found.");

        taxRate.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateTaxRateCommandHandler.MapToDto(taxRate);
    }
}
