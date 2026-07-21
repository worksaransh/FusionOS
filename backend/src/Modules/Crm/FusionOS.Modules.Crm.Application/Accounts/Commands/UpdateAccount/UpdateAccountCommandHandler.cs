using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.UpdateAccount;

public sealed class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountCommandHandler(IAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.CompanyId, request.AccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account '{request.AccountId}' was not found.");

        account.UpdateDetails(request.Name, request.Industry, request.Website);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToDto(account);
    }
}
