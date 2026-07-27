using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Domain.TaxRates;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.ReclassifyTaxRateGstComponent;

public sealed class ReclassifyTaxRateGstComponentCommandHandler : IRequestHandler<ReclassifyTaxRateGstComponentCommand, TaxRateDto>
{
    private readonly ITaxRateRepository _repository;
    private readonly FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork _unitOfWork;

    public ReclassifyTaxRateGstComponentCommandHandler(ITaxRateRepository repository, FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaxRateDto> Handle(ReclassifyTaxRateGstComponentCommand request, CancellationToken cancellationToken)
    {
        var taxRate = await _repository.GetByIdAsync(request.CompanyId, request.TaxRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tax rate '{request.TaxRateId}' was not found.");

        if (!Enum.TryParse<GstComponent>(request.GstComponent, ignoreCase: true, out var gstComponent))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.GstComponent), $"Unknown GST component '{request.GstComponent}'."),
            });
        }

        taxRate.ReclassifyGstComponent(gstComponent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateTaxRateCommandHandler.MapToDto(taxRate);
    }
}
