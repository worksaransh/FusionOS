using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BudgetLines.Commands.CreateBudgetLine;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Queries.ListBudgetLines;

public sealed class ListBudgetLinesQueryHandler : IRequestHandler<ListBudgetLinesQuery, PagedResult<BudgetLineDto>>
{
    private readonly IBudgetLineRepository _repository;

    public ListBudgetLinesQueryHandler(IBudgetLineRepository repository) => _repository = repository;

    public async Task<PagedResult<BudgetLineDto>> Handle(ListBudgetLinesQuery request, CancellationToken cancellationToken)
    {
        var budgetLines = await _repository.ListByBudgetAsync(request.CompanyId, request.BudgetId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByBudgetAsync(request.CompanyId, request.BudgetId, cancellationToken);

        var dtos = budgetLines.Select(CreateBudgetLineCommandHandler.MapToDto).ToList();

        return new PagedResult<BudgetLineDto>(dtos, request.Page, request.PageSize, total);
    }
}
