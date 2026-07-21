using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.DeactivateAccount;

public sealed class DeactivateAccountCommandHandler : IRequestHandler<DeactivateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAccountCommandHandler(IAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.CompanyId, request.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account '{request.AccountId}' was not found.");

        account.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToDto(account);
    }
}
