using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.CreateWithholdingTaxRate;

public sealed class CreateWithholdingTaxRateCommandHandler : IRequestHandler<CreateWithholdingTaxRateCommand, WithholdingTaxRateDto>
{
    private readonly IWithholdingTaxRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWithholdingTaxRateCommandHandler(IWithholdingTaxRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WithholdingTaxRateDto> Handle(CreateWithholdingTaxRateCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.SectionCodeExistsAsync(request.CompanyId, request.SectionCode, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SectionCode), $"Withholding tax section code '{request.SectionCode}' already exists for this company."),
            });
        }

        var rate = Domain.WithholdingTaxRates.WithholdingTaxRate.Create(request.CompanyId, request.SectionCode, request.Description, request.Percentage);

        await _repository.AddAsync(rate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rate);
    }

    internal static WithholdingTaxRateDto MapToDto(Domain.WithholdingTaxRates.WithholdingTaxRate rate) =>
        new(rate.Id, rate.SectionCode, rate.Description, rate.Percentage, rate.IsActive);
}
