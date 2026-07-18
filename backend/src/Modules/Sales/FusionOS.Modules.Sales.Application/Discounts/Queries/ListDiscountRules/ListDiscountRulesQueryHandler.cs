using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Discounts.Queries.ListDiscountRules;

public sealed class ListDiscountRulesQueryHandler : IRequestHandler<ListDiscountRulesQuery, PagedResult<DiscountRuleDto>>
{
    private readonly IDiscountRuleRepository _repository;

    public ListDiscountRulesQueryHandler(IDiscountRuleRepository repository) => _repository = repository;

    public async Task<PagedResult<DiscountRuleDto>> Handle(ListDiscountRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.ListAsync(request.CompanyId, request.ProductId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.ProductId, cancellationToken);

        var dtos = rules.Select(DiscountRuleMapper.ToDto).ToList();

        return new PagedResult<DiscountRuleDto>(dtos, request.Page, request.PageSize, total);
    }
}
