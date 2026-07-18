using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Companies.Commands.UpdateCompany;

public sealed class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto?>
{
    private readonly ICompanyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;

    public UpdateCompanyCommandHandler(ICompanyRepository repository, IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CompanyDto?> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.CompanyId is not { } companyId || companyId != request.Id)
            return null;

        var company = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (company is null)
            return null;

        company.UpdateDetails(request.Name, request.LegalName, request.TaxId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompanyDto(company.Id, company.Name, company.LegalName, company.TaxId, company.BaseCurrency, company.IsActive, company.CreatedAt);
    }
}
