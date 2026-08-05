using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.CreateWithholdingTaxRate;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.DeactivateWithholdingTaxRate;

public sealed class DeactivateWithholdingTaxRateCommandHandler : IRequestHandler<DeactivateWithholdingTaxRateCommand, WithholdingTaxRateDto>
{
    private readonly IWithholdingTaxRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateWithholdingTaxRateCommandHandler(IWithholdingTaxRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WithholdingTaxRateDto> Handle(DeactivateWithholdingTaxRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _repository.GetByIdAsync(request.CompanyId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Withholding tax rate '{request.Id}' was not found.");

        rate.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateWithholdingTaxRateCommandHandler.MapToDto(rate);
    }
}
