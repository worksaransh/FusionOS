using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.ListMigrationJobs;

public sealed class ListMigrationJobsQueryHandler : IRequestHandler<ListMigrationJobsQuery, PagedResult<MigrationJobDto>>
{
    private readonly IMigrationJobRepository _repository;

    public ListMigrationJobsQueryHandler(IMigrationJobRepository repository) => _repository = repository;

    public async Task<PagedResult<MigrationJobDto>> Handle(ListMigrationJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = jobs.Select(MigrationJobMapper.ToDto).ToList();

        return new PagedResult<MigrationJobDto>(dtos, request.Page, request.PageSize, total);
    }
}
