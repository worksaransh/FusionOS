using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Queries.GetFinanceSettings;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Settings.Commands.UpdateFinanceSettings;

/// <summary>
/// Validates every supplied account id exists (only, not that it's the
/// "right" type of account — e.g. nothing stops an admin from pointing
/// DefaultArAccountId at an Expense account; that's a chart-of-accounts
/// design mistake, not a shape violation this handler can catch) before
/// saving, same "validate at configuration time, trust it at posting time"
/// split as PostMonthlyDepreciationCommandHandler.
/// </summary>
public sealed class UpdateFinanceSettingsCommandHandler : IRequestHandler<UpdateFinanceSettingsCommand, FinanceSettingsDto>
{
    private readonly IFinanceSettingsRepository _repository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFinanceSettingsCommandHandler(IFinanceSettingsRepository repository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FinanceSettingsDto> Handle(UpdateFinanceSettingsCommand request, CancellationToken cancellationToken)
    {
        await EnsureAccountExistsAsync(request.CompanyId, request.DefaultArAccountId, nameof(request.DefaultArAccountId), cancellationToken);
        await EnsureAccountExistsAsync(request.CompanyId, request.DefaultSalesRevenueAccountId, nameof(request.DefaultSalesRevenueAccountId), cancellationToken);
        await EnsureAccountExistsAsync(request.CompanyId, request.DefaultApAccountId, nameof(request.DefaultApAccountId), cancellationToken);
        await EnsureAccountExistsAsync(request.CompanyId, request.DefaultPurchaseExpenseAccountId, nameof(request.DefaultPurchaseExpenseAccountId), cancellationToken);

        var settings = await _repository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        if (settings is null)
        {
            settings = Domain.Settings.FinanceSettings.CreateDefault(request.CompanyId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        settings.ConfigureAccounts(request.DefaultArAccountId, request.DefaultSalesRevenueAccountId, request.DefaultApAccountId, request.DefaultPurchaseExpenseAccountId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetFinanceSettingsQueryHandler.MapToDto(settings);
    }

    private async Task EnsureAccountExistsAsync(Guid companyId, Guid? accountId, string property, CancellationToken cancellationToken)
    {
        if (accountId is not { } id)
            return;

        if (!await _accountRepository.ExistsAsync(companyId, id, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(property, $"Account '{id}' does not exist for this company."),
            });
        }
    }
}
