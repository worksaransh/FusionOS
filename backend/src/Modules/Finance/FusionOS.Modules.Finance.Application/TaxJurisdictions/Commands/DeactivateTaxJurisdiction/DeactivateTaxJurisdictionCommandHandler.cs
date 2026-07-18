using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.DeactivateTaxJurisdiction;

public sealed class DeactivateTaxJurisdictionCommandHandler : IRequestHandler<DeactivateTaxJurisdictionCommand, TaxJurisdictionDto>
{
    private readonly ITaxJurisdictionRepository _repository;
    private readonly FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork _unitOfWork;

    public DeactivateTaxJurisdictionCommandHandler(ITaxJurisdictionRepository repository, FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaxJurisdictionDto> Handle(DeactivateTaxJurisdictionCommand request, CancellationToken cancellationToken)
    {
        var jurisdiction = await _repository.GetByIdAsync(request.CompanyId, request.TaxJurisdictionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tax jurisdiction '{request.TaxJurisdictionId}' was not found.");

        jurisdiction.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateTaxJurisdictionCommandHandler.MapToDto(jurisdiction);
    }
}
