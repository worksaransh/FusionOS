using FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Budgets.Queries.GetBudgetById;

public sealed class GetBudgetByIdQueryHandler : IRequestHandler<GetBudgetByIdQuery, BudgetDto>
{
    private readonly IBudgetRepository _repository;

    public GetBudgetByIdQueryHandler(IBudgetRepository repository) => _repository = repository;

    public async Task<BudgetDto> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var budget = await _repository.GetByIdAsync(request.CompanyId, request.BudgetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Budget '{request.BudgetId}' was not found.");

        return CreateBudgetCommandHandler.MapToDto(budget);
    }
}
