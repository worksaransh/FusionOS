using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ExtractTallyData;

public sealed class ExtractTallyDataCommandHandler : IRequestHandler<ExtractTallyDataCommand, MigrationJobDto>
{
    private readonly IMigrationJobRepository _jobRepository;
    private readonly IMigrationStagingRecordRepository _stagingRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Stateless, pure-parsing helper — instantiated directly rather than registered in DI,
    // same treatment as any other plain-CLR-object utility this codebase doesn't wire through the container.
    private readonly TallyXmlExtractor _extractor = new();

    public ExtractTallyDataCommandHandler(
        IMigrationJobRepository jobRepository,
        IMigrationStagingRecordRepository stagingRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _stagingRepository = stagingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MigrationJobDto> Handle(ExtractTallyDataCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.CompanyId, request.MigrationJobId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.MigrationJobId), $"Migration job '{request.MigrationJobId}' does not exist for this company."),
            });

        job.StartExtraction();

        IReadOnlyList<(string RecordType, string RawDataJson)> extracted;
        try
        {
            extracted = _extractor.Extract(request.TallyXmlContent);
        }
        catch (InvalidOperationException ex)
        {
            job.Fail($"Tally extraction failed: {ex.Message}");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.TallyXmlContent), ex.Message),
            });
        }

        var stagingRecords = extracted
            .Select(r => MigrationStagingRecord.Stage(request.CompanyId, request.MigrationJobId, r.RecordType, r.RawDataJson))
            .ToList();

        await _stagingRepository.AddRangeAsync(stagingRecords, cancellationToken);
        job.CompleteExtraction(stagingRecords.Count);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MigrationJobMapper.ToDto(job);
    }
}
