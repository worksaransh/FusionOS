using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.GetTaxJurisdictionById;

public sealed class GetTaxJurisdictionByIdQueryHandler : IRequestHandler<GetTaxJurisdictionByIdQuery, TaxJurisdictionDto>
{
    private readonly ITaxJurisdictionRepository _repository;

    public GetTaxJurisdictionByIdQueryHandler(ITaxJurisdictionRepository repository) => _repository = repository;

    public async Task<TaxJurisdictionDto> Handle(GetTaxJurisdictionByIdQuery request, CancellationToken cancellationToken)
    {
        var jurisdiction = await _repository.GetByIdAsync(request.CompanyId, request.TaxJurisdictionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tax jurisdiction '{request.TaxJurisdictionId}' was not found.");

        return CreateTaxJurisdictionCommandHandler.MapToDto(jurisdiction);
    }
}
