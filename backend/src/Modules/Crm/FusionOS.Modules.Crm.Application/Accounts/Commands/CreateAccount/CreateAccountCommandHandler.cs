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
        // Duplicate check runs BEFORE creating, but is advisory only — see AccountDto.
        // IsPossibleDuplicate. Two different real organizations can legitimately share a
        // name (e.g. franchises, or two unrelated companies that happen to be named the
        // same), so this warns rather than blocks — unlike a genuine uniqueness constraint
        // enforced elsewhere in this codebase (e.g. Product.Sku), where a collision really
        // is invalid. Account has no Email field, so Name is the normalized signal checked
        // here (case-insensitive, trimmed).
        var normalizedName = request.Name.Trim().ToLowerInvariant();
        var isPossibleDuplicate = await _repository.IsNameAlreadyUsedAsync(request.CompanyId, normalizedName, cancellationToken);

        var account = Domain.Accounts.Account.Create(request.CompanyId, request.Name, request.Industry, request.Website);

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountMapper.ToDto(account, isPossibleDuplicate);
    }
}
