using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Domain.Budgets;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;

public sealed class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, BudgetDto>
{
    private readonly IBudgetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBudgetCommandHandler(IBudgetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = Budget.Create(request.CompanyId, request.Name, request.PeriodStart, request.PeriodEnd);

        await _repository.AddAsync(budget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(budget);
    }

    internal static BudgetDto MapToDto(Budget budget) => new(
        budget.Id, budget.Name, budget.PeriodStart, budget.PeriodEnd, budget.IsActive, budget.CreatedAt);
}
