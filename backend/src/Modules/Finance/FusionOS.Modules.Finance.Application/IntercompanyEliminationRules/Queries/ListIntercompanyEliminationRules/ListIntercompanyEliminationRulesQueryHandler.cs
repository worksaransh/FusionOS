using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.CreateIntercompanyEliminationRule;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Queries.ListIntercompanyEliminationRules;

public sealed class ListIntercompanyEliminationRulesQueryHandler : IRequestHandler<ListIntercompanyEliminationRulesQuery, PagedResult<IntercompanyEliminationRuleDto>>
{
    private readonly IIntercompanyEliminationRuleRepository _repository;

    public ListIntercompanyEliminationRulesQueryHandler(IIntercompanyEliminationRuleRepository repository) => _repository = repository;

    public async Task<PagedResult<IntercompanyEliminationRuleDto>> Handle(ListIntercompanyEliminationRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        return new PagedResult<IntercompanyEliminationRuleDto>(
            rules.Select(CreateIntercompanyEliminationRuleCommandHandler.MapToDto).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
