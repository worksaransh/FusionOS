using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Accounts.Queries.ListAccounts;

public sealed class ListAccountsQueryHandler : IRequestHandler<ListAccountsQuery, PagedResult<AccountDto>>
{
    private readonly IAccountRepository _repository;

    public ListAccountsQueryHandler(IAccountRepository repository) => _repository = repository;

    public async Task<PagedResult<AccountDto>> Handle(ListAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = accounts.Select(CreateAccountCommandHandler.MapToDto).ToList();

        return new PagedResult<AccountDto>(dtos, request.Page, request.PageSize, total);
    }
}
