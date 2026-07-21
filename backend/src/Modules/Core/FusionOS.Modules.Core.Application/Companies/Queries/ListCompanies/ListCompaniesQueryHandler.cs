using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Companies.Queries.ListCompanies;

public sealed class ListCompaniesQueryHandler : IRequestHandler<ListCompaniesQuery, PagedResult<CompanyDto>>
{
    private readonly ICompanyRepository _repository;
    private readonly ICurrentUserContext _currentUser;

    public ListCompaniesQueryHandler(ICompanyRepository repository, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    // Fixed a confirmed cross-tenant leak (2026-07 audit): this previously
    // returned every company on the entire platform to any authenticated user
    // regardless of which company they belong to — no permission gate, no
    // tenant filter at all. RBAC has no "platform admin" concept yet, so until
    // one exists, the only correct scope for this endpoint is "the caller's own
    // company", matching how every other List query in the codebase is scoped
    // via TenantIsolationBehavior's CompanyId check. ListCompaniesQuery itself
    // carries no CompanyId property (it lists the tenant root, not a record
    // *within* a tenant), so that shared pipeline behavior can't apply here —
    // this handler enforces the equivalent restriction directly instead.
    public async Task<PagedResult<CompanyDto>> Handle(ListCompaniesQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.CompanyId is not { } companyId)
            return new PagedResult<CompanyDto>(Array.Empty<CompanyDto>(), request.Page, request.PageSize, 0);

        var company = await _repository.GetByIdAsync(companyId, cancellationToken);
        if (company is null)
            return new PagedResult<CompanyDto>(Array.Empty<CompanyDto>(), request.Page, request.PageSize, 0);

        var dto = new CompanyDto(company.Id, company.Name, company.LegalName, company.TaxId, company.BaseCurrency, company.IsActive, company.CreatedAt);
        return new PagedResult<CompanyDto>(new[] { dto }, request.Page, request.PageSize, 1);
    }
}
