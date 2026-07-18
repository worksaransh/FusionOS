using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Settings.Contracts;
using FusionOS.Modules.Core.Application.Settings.Queries.GetCompanySettings;
using MediatR;

namespace FusionOS.Modules.Core.Application.Settings.Commands.UpdateCompanySettings;

public sealed class UpdateCompanySettingsCommandHandler : IRequestHandler<UpdateCompanySettingsCommand, CompanySettingsDto>
{
    private readonly ICompanySettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCompanySettingsCommandHandler(ICompanySettingsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CompanySettingsDto> Handle(UpdateCompanySettingsCommand request, CancellationToken cancellationToken)
    {
        // Same get-or-create as GetCompanySettingsQueryHandler — an Update can
        // legitimately be the very first write if the caller never issued a Get
        // first (e.g. a script calling the API directly).
        var settings = await _repository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        if (settings is null)
        {
            settings = Domain.Settings.CompanySettings.CreateDefault(request.CompanyId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        settings.UpdateSettings(request.DefaultCurrency, request.DefaultPageSize, request.DisplayName, request.LogoUrl);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetCompanySettingsQueryHandler.MapToDto(settings);
    }
}
