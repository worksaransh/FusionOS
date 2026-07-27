using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.ListMigrationStagingRecordsForJob;

public sealed class ListMigrationStagingRecordsForJobQueryHandler : IRequestHandler<ListMigrationStagingRecordsForJobQuery, PagedResult<MigrationStagingRecordDto>>
{
    private readonly IMigrationStagingRecordRepository _repository;

    public ListMigrationStagingRecordsForJobQueryHandler(IMigrationStagingRecordRepository repository) => _repository = repository;

    public async Task<PagedResult<MigrationStagingRecordDto>> Handle(ListMigrationStagingRecordsForJobQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.ListByJobIdAsync(request.CompanyId, request.MigrationJobId, request.MappingStatus, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByJobIdAsync(request.CompanyId, request.MigrationJobId, request.MappingStatus, cancellationToken);

        var dtos = records.Select(MigrationStagingRecordMapper.ToDto).ToList();

        return new PagedResult<MigrationStagingRecordDto>(dtos, request.Page, request.PageSize, total);
    }
}
