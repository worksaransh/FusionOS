using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Budgets.Commands.DeactivateBudget;

public sealed class DeactivateBudgetCommandHandler : IRequestHandler<DeactivateBudgetCommand, BudgetDto>
{
    private readonly IBudgetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBudgetCommandHandler(IBudgetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BudgetDto> Handle(DeactivateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _repository.GetByIdAsync(request.CompanyId, request.BudgetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Budget '{request.BudgetId}' was not found.");

        budget.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateBudgetCommandHandler.MapToDto(budget);
    }
}
