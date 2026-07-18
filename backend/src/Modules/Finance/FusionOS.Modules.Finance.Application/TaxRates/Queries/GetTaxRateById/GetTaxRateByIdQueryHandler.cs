using FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxRates.Queries.GetTaxRateById;

public sealed class GetTaxRateByIdQueryHandler : IRequestHandler<GetTaxRateByIdQuery, TaxRateDto>
{
    private readonly ITaxRateRepository _repository;

    public GetTaxRateByIdQueryHandler(ITaxRateRepository repository) => _repository = repository;

    public async Task<TaxRateDto> Handle(GetTaxRateByIdQuery request, CancellationToken cancellationToken)
    {
        var taxRate = await _repository.GetByIdAsync(request.CompanyId, request.TaxRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tax rate '{request.TaxRateId}' was not found.");

        return CreateTaxRateCommandHandler.MapToDto(taxRate);
    }
}
