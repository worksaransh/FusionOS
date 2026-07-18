using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Budgets.Queries.ListBudgets;

public sealed class ListBudgetsQueryHandler : IRequestHandler<ListBudgetsQuery, PagedResult<BudgetDto>>
{
    private readonly IBudgetRepository _repository;

    public ListBudgetsQueryHandler(IBudgetRepository repository) => _repository = repository;

    public async Task<PagedResult<BudgetDto>> Handle(ListBudgetsQuery request, CancellationToken cancellationToken)
    {
        var budgets = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = budgets.Select(CreateBudgetCommandHandler.MapToDto).ToList();

        return new PagedResult<BudgetDto>(dtos, request.Page, request.PageSize, total);
    }
}
