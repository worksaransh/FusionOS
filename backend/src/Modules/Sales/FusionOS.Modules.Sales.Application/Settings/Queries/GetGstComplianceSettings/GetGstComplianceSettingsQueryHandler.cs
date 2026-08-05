using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Settings.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Settings.Queries.GetGstComplianceSettings;

public sealed class GetGstComplianceSettingsQueryHandler : IRequestHandler<GetGstComplianceSettingsQuery, GstComplianceSettingsDto>
{
    private readonly IGstComplianceSettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetGstComplianceSettingsQueryHandler(IGstComplianceSettingsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GstComplianceSettingsDto> Handle(GetGstComplianceSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        if (settings is null)
        {
            settings = Domain.Settings.GstComplianceSettings.CreateDefault(request.CompanyId);
            await _repository.AddAsync(settings, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(settings);
    }

    internal static GstComplianceSettingsDto MapToDto(Domain.Settings.GstComplianceSettings settings) =>
        new(settings.CompanyId, settings.EInvoicingRequired);
}
