using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Settings.Contracts;
using FusionOS.Modules.Sales.Application.Settings.Queries.GetGstComplianceSettings;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Settings.Commands.UpdateGstComplianceSettings;

public sealed class UpdateGstComplianceSettingsCommandHandler : IRequestHandler<UpdateGstComplianceSettingsCommand, GstComplianceSettingsDto>
{
    private readonly IGstComplianceSettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateGstComplianceSettingsCommandHandler(IGstComplianceSettingsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GstComplianceSettingsDto> Handle(UpdateGstComplianceSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        if (settings is null)
        {
            settings = Domain.Settings.GstComplianceSettings.CreateDefault(request.CompanyId);
            await _repository.AddAsync(settings, cancellationToken);
        }

        settings.Configure(request.EInvoicingRequired);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetGstComplianceSettingsQueryHandler.MapToDto(settings);
    }
}
