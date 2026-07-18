using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Budgets.Contracts;
using FusionOS.Modules.Finance.Application.BudgetLines.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Domain.BudgetLines;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BudgetLines.Commands.CreateBudgetLine;

/// <summary>
/// Validates the parent Budget, the Account, and (if supplied) the
/// CostCenter all exist for this company before creating the line — same
/// handler-level existence-check split CreateJournalEntryCommandHandler uses
/// for JournalEntryLine.AccountId, applied here to three references instead
/// of one.
/// </summary>
public sealed class CreateBudgetLineCommandHandler : IRequestHandler<CreateBudgetLineCommand, BudgetLineDto>
{
    private readonly IBudgetLineRepository _repository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBudgetLineCommandHandler(
        IBudgetLineRepository repository,
        IBudgetRepository budgetRepository,
        IAccountRepository accountRepository,
        ICostCenterRepository costCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _budgetRepository = budgetRepository;
        _accountRepository = accountRepository;
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BudgetLineDto> Handle(CreateBudgetLineCommand request, CancellationToken cancellationToken)
    {
        if (!await _budgetRepository.ExistsAsync(request.CompanyId, request.BudgetId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BudgetId), $"Budget '{request.BudgetId}' does not exist for this company."),
            });
        }

        if (!await _accountRepository.ExistsAsync(request.CompanyId, request.AccountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AccountId), $"Account '{request.AccountId}' does not exist for this company."),
            });
        }

        if (request.CostCenterId.HasValue &&
            await _costCenterRepository.GetByIdAsync(request.CompanyId, request.CostCenterId.Value, cancellationToken) is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CostCenterId), $"Cost center '{request.CostCenterId}' does not exist for this company."),
            });
        }

        var budgetLine = BudgetLine.Create(request.CompanyId, request.BudgetId, request.AccountId, request.CostCenterId, request.BudgetedAmount, request.Notes);

        await _repository.AddAsync(budgetLine, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(budgetLine);
    }

    internal static BudgetLineDto MapToDto(BudgetLine budgetLine) => new(
        budgetLine.Id, budgetLine.BudgetId, budgetLine.AccountId, budgetLine.CostCenterId,
        budgetLine.BudgetedAmount, budgetLine.Notes, budgetLine.CreatedAt);
}
