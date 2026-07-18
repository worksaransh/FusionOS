using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Domain.Accounts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Accounts.Commands.UpdateAccount;

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

        if (request.ParentAccountId is { } parentId && !await _repository.ExistsAsync(request.CompanyId, parentId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ParentAccountId), "Parent account does not exist for this company."),
            });
        }

        var accountType = Enum.Parse<AccountType>(request.AccountType);
        account.UpdateDetails(request.Name, accountType, request.ParentAccountId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateAccountCommandHandler.MapToDto(account);
    }
}
