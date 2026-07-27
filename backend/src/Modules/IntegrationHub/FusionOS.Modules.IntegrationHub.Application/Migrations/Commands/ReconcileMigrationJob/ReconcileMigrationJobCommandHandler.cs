using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ReconcileMigrationJob;

public sealed class ReconcileMigrationJobCommandHandler : IRequestHandler<ReconcileMigrationJobCommand, MigrationJobDto>
{
    private readonly IMigrationJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReconcileMigrationJobCommandHandler(IMigrationJobRepository jobRepository, IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MigrationJobDto> Handle(ReconcileMigrationJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.CompanyId, request.MigrationJobId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.MigrationJobId), $"Migration job '{request.MigrationJobId}' does not exist for this company."),
            });

        var extracted = job.TotalRecordsExtracted;
        var loaded = job.TotalRecordsLoaded;
        var failed = job.TotalRecordsFailed;
        var accountedFor = loaded + failed;

        var note = $"{loaded}/{extracted} records loaded successfully ({failed} failed validation/load — see staging records for details). " +
                   (accountedFor == extracted ? "Reconciliation matches." : "MISMATCH — some records unaccounted for.");

        job.Reconcile(note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MigrationJobMapper.ToDto(job);
    }
}
