using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.SodConflictRules.Commands.CreateSodConflictRule;
using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Queries.ListSodConflictRules;

public sealed class ListSodConflictRulesQueryHandler : IRequestHandler<ListSodConflictRulesQuery, PagedResult<SodConflictRuleDto>>
{
    private readonly ISodConflictRuleRepository _repository;

    public ListSodConflictRulesQueryHandler(ISodConflictRuleRepository repository) => _repository = repository;

    public async Task<PagedResult<SodConflictRuleDto>> Handle(ListSodConflictRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        return new PagedResult<SodConflictRuleDto>(
            rules.Select(CreateSodConflictRuleCommandHandler.MapToDto).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
