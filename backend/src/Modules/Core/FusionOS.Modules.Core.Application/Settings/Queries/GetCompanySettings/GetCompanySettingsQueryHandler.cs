using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Settings.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Settings.Queries.GetCompanySettings;

public sealed class GetCompanySettingsQueryHandler : IRequestHandler<GetCompanySettingsQuery, CompanySettingsDto>
{
    private readonly ICompanySettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetCompanySettingsQueryHandler(ICompanySettingsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompanySettingsDto> Handle(GetCompanySettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);

        // Get-or-create (Phase M5): every company has settings from the moment
        // this is first read, even if nobody has ever opened the Settings page.
        if (settings is null)
        {
            settings = Domain.Settings.CompanySettings.CreateDefault(request.CompanyId);
            await _repository.AddAsync(settings, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(settings);
    }

    internal static CompanySettingsDto MapToDto(Domain.Settings.CompanySettings settings) => new(
        settings.CompanyId, settings.DefaultCurrency, settings.DefaultPageSize, settings.DisplayName, settings.LogoUrl);
}
