using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Companies.Queries.ListCompanies;

public sealed class ListCompaniesQueryHandler : IRequestHandler<ListCompaniesQuery, PagedResult<CompanyDto>>
{
    private readonly ICompanyRepository _repository;

    public ListCompaniesQueryHandler(ICompanyRepository repository) => _repository = repository;

    public async Task<PagedResult<CompanyDto>> Handle(ListCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _repository.ListAsync(request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(cancellationToken);

        var dtos = companies
            .Select(c => new CompanyDto(c.Id, c.Name, c.LegalName, c.TaxId, c.BaseCurrency, c.IsActive, c.CreatedAt))
            .ToList();

        return new PagedResult<CompanyDto>(dtos, request.Page, request.PageSize, total);
    }
}
