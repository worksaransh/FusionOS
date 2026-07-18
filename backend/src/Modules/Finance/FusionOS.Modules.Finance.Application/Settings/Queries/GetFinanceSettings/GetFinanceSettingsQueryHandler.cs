using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Settings.Queries.GetFinanceSettings;

public sealed class GetFinanceSettingsQueryHandler : IRequestHandler<GetFinanceSettingsQuery, FinanceSettingsDto>
{
    private readonly IFinanceSettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetFinanceSettingsQueryHandler(IFinanceSettingsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FinanceSettingsDto> Handle(GetFinanceSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        if (settings is null)
        {
            settings = Domain.Settings.FinanceSettings.CreateDefault(request.CompanyId);
            await _repository.AddAsync(settings, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(settings);
    }

    internal static FinanceSettingsDto MapToDto(Domain.Settings.FinanceSettings settings) => new(
        settings.CompanyId,
        settings.DefaultArAccountId,
        settings.DefaultSalesRevenueAccountId,
        settings.DefaultApAccountId,
        settings.DefaultPurchaseExpenseAccountId);
}
