using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;

public sealed class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, BankAccountDto>
{
    private readonly IBankAccountRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBankAccountCommandHandler(IBankAccountRepository repository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankAccountDto> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        if (!await _accountRepository.ExistsAsync(request.CompanyId, request.LinkedAccountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.LinkedAccountId), "Linked GL account does not exist for this company."),
            });
        }

        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Bank account code '{request.Code}' already exists for this company."),
            });
        }

        var bankAccount = BankAccount.Create(request.CompanyId, request.Code, request.Name, request.LinkedAccountId, request.BankName, request.AccountNumberLast4);

        await _repository.AddAsync(bankAccount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(bankAccount);
    }

    internal static BankAccountDto MapToDto(BankAccount bankAccount) => new(
        bankAccount.Id, bankAccount.Code, bankAccount.Name, bankAccount.LinkedAccountId,
        bankAccount.BankName, bankAccount.AccountNumberLast4, bankAccount.IsActive, bankAccount.CreatedAt);
}
