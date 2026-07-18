using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;

public sealed class CreateTaxJurisdictionCommandHandler : IRequestHandler<CreateTaxJurisdictionCommand, TaxJurisdictionDto>
{
    private readonly ITaxJurisdictionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaxJurisdictionCommandHandler(ITaxJurisdictionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TaxJurisdictionDto> Handle(CreateTaxJurisdictionCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Tax jurisdiction code '{request.Code}' already exists for this company."),
            });
        }

        var jurisdiction = TaxJurisdiction.Create(request.CompanyId, request.Code, request.Name);

        await _repository.AddAsync(jurisdiction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(jurisdiction);
    }

    internal static TaxJurisdictionDto MapToDto(TaxJurisdiction jurisdiction) => new(
        jurisdiction.Id, jurisdiction.Code, jurisdiction.Name, jurisdiction.IsActive, jurisdiction.CreatedAt);
}
