using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;

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
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Account code '{request.Code}' already exists for this company."),
            });
        }

        if (request.ParentAccountId is { } parentId && !await _repository.ExistsAsync(request.CompanyId, parentId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ParentAccountId), "Parent account does not exist for this company."),
            });
        }

        var accountType = Enum.Parse<AccountType>(request.AccountType);
        var account = Account.Create(request.CompanyId, request.Code, request.Name, accountType, request.ParentAccountId);

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(account);
    }

    internal static AccountDto MapToDto(Account account) => new(
        account.Id, account.Code, account.Name, account.AccountType.ToString(), account.ParentAccountId, account.IsActive, account.CreatedAt);
}
