using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankAccounts.Commands.CreateBankAccount;
using FusionOS.Modules.Finance.Application.BankAccounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.BankAccounts.Queries.ListBankAccounts;

public sealed class ListBankAccountsQueryHandler : IRequestHandler<ListBankAccountsQuery, PagedResult<BankAccountDto>>
{
    private readonly IBankAccountRepository _repository;

    public ListBankAccountsQueryHandler(IBankAccountRepository repository) => _repository = repository;

    public async Task<PagedResult<BankAccountDto>> Handle(ListBankAccountsQuery request, CancellationToken cancellationToken)
    {
        var bankAccounts = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = bankAccounts.Select(CreateBankAccountCommandHandler.MapToDto).ToList();

        return new PagedResult<BankAccountDto>(dtos, request.Page, request.PageSize, total);
    }
}
