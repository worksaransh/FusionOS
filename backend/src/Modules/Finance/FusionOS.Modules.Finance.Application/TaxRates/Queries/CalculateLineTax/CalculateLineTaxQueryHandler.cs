using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxRates.Queries.CalculateLineTax;

public sealed class CalculateLineTaxQueryHandler : IRequestHandler<CalculateLineTaxQuery, LineTaxResultDto>
{
    private readonly ITaxRateRepository _repository;

    public CalculateLineTaxQueryHandler(ITaxRateRepository repository) => _repository = repository;

    public async Task<LineTaxResultDto> Handle(CalculateLineTaxQuery request, CancellationToken cancellationToken)
    {
        if (request.NetAmount < 0)
            throw new ArgumentException("Net amount cannot be negative.", nameof(request.NetAmount));

        var taxRate = await _repository.GetByIdAsync(request.CompanyId, request.TaxRateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tax rate '{request.TaxRateId}' was not found.");

        // Round to the same numeric(19,4) scale the line columns persist at, so the
        // computed TaxAmount a caller stores is exactly what this query returned —
        // no silent re-rounding at the database boundary.
        var taxAmount = Math.Round(request.NetAmount * taxRate.Percentage / 100m, 4, MidpointRounding.AwayFromZero);

        return new LineTaxResultDto(
            request.NetAmount,
            taxRate.Id,
            taxRate.Code,
            taxRate.Percentage,
            taxAmount,
            request.NetAmount + taxAmount);
    }
}
