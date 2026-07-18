using FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Queries.GetBankAccountById;

public sealed class GetBankAccountByIdQueryHandler : IRequestHandler<GetBankAccountByIdQuery, BankAccountDto>
{
    private readonly IBankAccountRepository _repository;

    public GetBankAccountByIdQueryHandler(IBankAccountRepository repository) => _repository = repository;

    public async Task<BankAccountDto> Handle(GetBankAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var bankAccount = await _repository.GetByIdAsync(request.CompanyId, request.BankAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bank account '{request.BankAccountId}' was not found.");

        return CreateBankAccountCommandHandler.MapToDto(bankAccount);
    }
}
