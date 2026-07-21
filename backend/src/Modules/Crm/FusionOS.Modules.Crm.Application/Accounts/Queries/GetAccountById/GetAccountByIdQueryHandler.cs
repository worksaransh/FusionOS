using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Accounts.Queries.GetAccountById;

public sealed class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IAccountRepository _repository;

    public GetAccountByIdQueryHandler(IAccountRepository repository) => _repository = repository;

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.CompanyId, request.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account '{request.AccountId}' was not found.");

        return AccountMapper.ToDto(account);
    }
}
