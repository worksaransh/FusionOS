using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Companies.Queries.GetCompanyById;

public sealed class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto?>
{
    private readonly ICompanyRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public GetCompanyByIdQueryHandler(ICompanyRepository repository, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<CompanyDto?> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.CompanyId is not { } companyId || companyId != request.Id)
            return null;

        var company = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (company is null)
            return null;

        return new CompanyDto(company.Id, company.Name, company.LegalName, company.TaxId, company.BaseCurrency, company.IsActive, company.CreatedAt);
    }
}
