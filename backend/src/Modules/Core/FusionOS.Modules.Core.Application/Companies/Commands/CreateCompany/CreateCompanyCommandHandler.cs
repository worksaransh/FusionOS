using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Companies;
using MediatR;

namespace FusionOS.Modules.Core.Application.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCompanyCommandHandler(ICompanyRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = Company.Create(request.Name, request.LegalName, request.BaseCurrency, request.TaxId);

        await _repository.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompanyDto(company.Id, company.Name, company.LegalName, company.TaxId, company.BaseCurrency, company.IsActive, company.CreatedAt);
    }
}
