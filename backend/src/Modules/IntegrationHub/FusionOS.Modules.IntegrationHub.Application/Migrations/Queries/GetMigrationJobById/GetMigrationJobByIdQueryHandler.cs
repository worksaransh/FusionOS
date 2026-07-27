using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.GetMigrationJobById;

public sealed class GetMigrationJobByIdQueryHandler : IRequestHandler<GetMigrationJobByIdQuery, MigrationJobDto>
{
    private readonly IMigrationJobRepository _repository;

    public GetMigrationJobByIdQueryHandler(IMigrationJobRepository repository) => _repository = repository;

    public async Task<MigrationJobDto> Handle(GetMigrationJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.CompanyId, request.MigrationJobId, cancellationToken)
            ?? throw new KeyNotFoundException($"Migration job '{request.MigrationJobId}' was not found.");

        return MigrationJobMapper.ToDto(job);
    }
}
