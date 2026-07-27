using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Queries.ListMeterReadingsForAsset;

public sealed class ListMeterReadingsForAssetQueryHandler : IRequestHandler<ListMeterReadingsForAssetQuery, PagedResult<MeterReadingDto>>
{
    private readonly IMeterReadingRepository _repository;

    public ListMeterReadingsForAssetQueryHandler(IMeterReadingRepository repository) => _repository = repository;

    public async Task<PagedResult<MeterReadingDto>> Handle(ListMeterReadingsForAssetQuery request, CancellationToken cancellationToken)
    {
        var readings = await _repository.ListForAssetAsync(request.CompanyId, request.AssetId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountForAssetAsync(request.CompanyId, request.AssetId, cancellationToken);

        var dtos = readings.Select(MeterReadingMapper.ToDto).ToList();

        return new PagedResult<MeterReadingDto>(dtos, request.Page, request.PageSize, total);
    }
}
