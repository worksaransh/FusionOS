using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Commands.CreateBudget;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Budgets.Commands.UpdateBudget;

public sealed class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, BudgetDto>
{
    private readonly IBudgetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBudgetCommandHandler(IBudgetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BudgetDto> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var budget = await _repository.GetByIdAsync(request.CompanyId, request.BudgetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Budget '{request.BudgetId}' was not found.");

        budget.UpdateDetails(request.Name, request.PeriodStart, request.PeriodEnd);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateBudgetCommandHandler.MapToDto(budget);
    }
}
