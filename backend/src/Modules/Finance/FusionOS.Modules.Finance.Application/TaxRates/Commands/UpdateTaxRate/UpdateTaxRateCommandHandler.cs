using FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.UpdateTaxRate;

public sealed class UpdateTaxRateCommandHandler : IRequestHandler<UpdateTaxRateCommand, TaxRateDto>
{
    private readonly ITaxRateRepository _repository;
    private readonly FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork _unitOfWork;

    public UpdateTaxRateCommandHandler(ITaxRateRepository repository, FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaxRateDto> Handle(UpdateTaxRateCommand request, CancellationToken cancellationToken)
    {
        var taxRate = await _repository.GetByIdAsync(request.CompanyId, request.TaxRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tax rate '{request.TaxRateId}' was not found.");

        taxRate.UpdateDetails(request.Name, request.Percentage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateTaxRateCommandHandler.MapToDto(taxRate);
    }
}
