using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Commands.RecordKpiSnapshot;

/// <summary>Validates the KpiDefinition exists for this company before recording a snapshot — same handler-level existence-check split CreateJournalEntryCommandHandler uses for JournalEntryLine.AccountId.</summary>
public sealed class RecordKpiSnapshotCommandHandler : IRequestHandler<RecordKpiSnapshotCommand, KpiSnapshotDto>
{
    private readonly IKpiSnapshotRepository _repository;
    private readonly IKpiDefinitionRepository _kpiDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordKpiSnapshotCommandHandler(IKpiSnapshotRepository repository, IKpiDefinitionRepository kpiDefinitionRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _kpiDefinitionRepository = kpiDefinitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<KpiSnapshotDto> Handle(RecordKpiSnapshotCommand request, CancellationToken cancellationToken)
    {
        if (!await _kpiDefinitionRepository.ExistsAsync(request.CompanyId, request.KpiDefinitionId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.KpiDefinitionId), $"KPI definition '{request.KpiDefinitionId}' does not exist for this company."),
            });
        }

        var snapshot = Domain.KpiSnapshots.KpiSnapshot.Create(request.CompanyId, request.KpiDefinitionId, request.Value, request.Notes);

        await _repository.AddAsync(snapshot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return KpiSnapshotMapper.ToDto(snapshot);
    }
}
