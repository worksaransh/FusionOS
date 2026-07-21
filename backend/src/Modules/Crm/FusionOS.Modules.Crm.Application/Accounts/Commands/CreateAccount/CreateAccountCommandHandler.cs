using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.CreateAccount;

public sealed class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(IAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = Domain.Accounts.Account.Create(request.CompanyId, request.Name, request.Industry, request.Website);

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToDto(account);
    }
}
