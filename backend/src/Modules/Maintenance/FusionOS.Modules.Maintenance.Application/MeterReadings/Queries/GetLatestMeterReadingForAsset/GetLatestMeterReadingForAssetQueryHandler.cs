using FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Queries.GetLatestMeterReadingForAsset;

public sealed class GetLatestMeterReadingForAssetQueryHandler : IRequestHandler<GetLatestMeterReadingForAssetQuery, MeterReadingDto?>
{
    private readonly IMeterReadingRepository _repository;

    public GetLatestMeterReadingForAssetQueryHandler(IMeterReadingRepository repository) => _repository = repository;

    public async Task<MeterReadingDto?> Handle(GetLatestMeterReadingForAssetQuery request, CancellationToken cancellationToken)
    {
        var reading = await _repository.GetLatestForAssetAsync(request.CompanyId, request.AssetId, cancellationToken);
        return reading is null ? null : MeterReadingMapper.ToDto(reading);
    }
}
