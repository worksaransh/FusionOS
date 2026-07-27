using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.CreateMigrationJob;

public sealed class CreateMigrationJobCommandHandler : IRequestHandler<CreateMigrationJobCommand, MigrationJobDto>
{
    private readonly IMigrationJobRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMigrationJobCommandHandler(IMigrationJobRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MigrationJobDto> Handle(CreateMigrationJobCommand request, CancellationToken cancellationToken)
    {
        var job = Domain.Migrations.MigrationJob.Create(request.CompanyId, request.SourceSystem);

        await _repository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MigrationJobMapper.ToDto(job);
    }
}
