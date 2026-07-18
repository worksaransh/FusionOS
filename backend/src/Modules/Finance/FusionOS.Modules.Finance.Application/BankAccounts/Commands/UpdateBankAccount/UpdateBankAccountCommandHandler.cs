using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Commands.UpdateBankAccount;

public sealed class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, BankAccountDto>
{
    private readonly IBankAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBankAccountCommandHandler(IBankAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BankAccountDto> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await _repository.GetByIdAsync(request.CompanyId, request.BankAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bank account '{request.BankAccountId}' was not found.");

        bankAccount.UpdateDetails(request.Name, request.BankName, request.AccountNumberLast4);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateBankAccountCommandHandler.MapToDto(bankAccount);
    }
}
