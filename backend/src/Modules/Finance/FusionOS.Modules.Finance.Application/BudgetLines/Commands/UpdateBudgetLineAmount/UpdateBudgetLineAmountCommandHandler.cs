using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BudgetLines.Commands.CreateBudgetLine;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Commands.UpdateBudgetLineAmount;

public sealed class UpdateBudgetLineAmountCommandHandler : IRequestHandler<UpdateBudgetLineAmountCommand, BudgetLineDto>
{
    private readonly IBudgetLineRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBudgetLineAmountCommandHandler(IBudgetLineRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BudgetLineDto> Handle(UpdateBudgetLineAmountCommand request, CancellationToken cancellationToken)
    {
        var budgetLine = await _repository.GetByIdAsync(request.CompanyId, request.BudgetLineId, cancellationToken)
            ?? throw new KeyNotFoundException($"Budget line '{request.BudgetLineId}' was not found.");

        budgetLine.UpdateAmount(request.BudgetedAmount, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateBudgetLineCommandHandler.MapToDto(budgetLine);
    }
}
