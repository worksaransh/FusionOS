using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.DeactivateBankAccount;

public sealed class DeactivateBankAccountCommandHandler : IRequestHandler<DeactivateBankAccountCommand, BankAccountDto>
{
    private readonly IBankAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBankAccountCommandHandler(IBankAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankAccountDto> Handle(DeactivateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await _repository.GetByIdAsync(request.CompanyId, request.BankAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bank account '{request.BankAccountId}' was not found.");

        bankAccount.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateBankAccountCommandHandler.MapToDto(bankAccount);
    }
}
