using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Domain.TaxRates;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;

public sealed class CreateTaxRateCommandHandler : IRequestHandler<CreateTaxRateCommand, TaxRateDto>
{
    private readonly ITaxRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaxRateCommandHandler(ITaxRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaxRateDto> Handle(CreateTaxRateCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.TaxJurisdictionExistsAsync(request.CompanyId, request.TaxJurisdictionId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.TaxJurisdictionId), "Tax jurisdiction does not exist for this company."),
            });
        }

        if (await _repository.CodeExistsAsync(request.CompanyId, request.TaxJurisdictionId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Tax rate code '{request.Code}' already exists in this jurisdiction."),
            });
        }

        if (!Enum.TryParse<GstComponent>(request.GstComponent, ignoreCase: true, out var gstComponent))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.GstComponent), $"Unknown GST component '{request.GstComponent}'."),
            });
        }

        var taxRate = TaxRate.Create(request.CompanyId, request.TaxJurisdictionId, request.Code, request.Name, request.Percentage, gstComponent);

        await _repository.AddAsync(taxRate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(taxRate);
    }

    internal static TaxRateDto MapToDto(TaxRate taxRate) => new(
        taxRate.Id, taxRate.TaxJurisdictionId, taxRate.Code, taxRate.Name, taxRate.Percentage, taxRate.IsActive, taxRate.CreatedAt, taxRate.GstComponent.ToString());
}
