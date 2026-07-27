using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;

public sealed class CreateInspectionCommandHandler : IRequestHandler<CreateInspectionCommand, InspectionDto>
{
    private readonly IInspectionRepository _repository;
    private readonly IInspectionPlanRepository _inspectionPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInspectionCommandHandler(IInspectionRepository repository, IInspectionPlanRepository inspectionPlanRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _inspectionPlanRepository = inspectionPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InspectionDto> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        // The plan is only ever a convenience default: explicit Characteristics, when given,
        // always win over whatever the plan defines — the plan just saves the caller from
        // hand-typing the usual list.
        var characteristics = request.Characteristics;
        if ((characteristics is null || characteristics.Count == 0) && request.InspectionPlanId.HasValue)
        {
            var plan = await _inspectionPlanRepository.GetByIdAsync(request.CompanyId, request.InspectionPlanId.Value, cancellationToken)
                ?? throw new KeyNotFoundException($"Inspection plan '{request.InspectionPlanId}' was not found.");

            characteristics = plan.Characteristics.OrderBy(c => c.SequenceNumber).Select(c => c.CharacteristicName).ToList();
        }

        // SPC control limits are matched onto the resolved characteristic list by name — same
        // rule whether the characteristics came from an explicit list or an InspectionPlan
        // default above.
        var limitsByCharacteristic = (request.ControlLimits ?? Array.Empty<InspectionCharacteristicControlLimits>())
            .ToDictionary(c => c.Characteristic?.Trim() ?? string.Empty, c => c, StringComparer.OrdinalIgnoreCase);

        var characteristicInputs = characteristics
            .Select(name =>
            {
                limitsByCharacteristic.TryGetValue(name?.Trim() ?? string.Empty, out var limits);
                return new InspectionCharacteristicInput(name!, limits?.LowerControlLimit, limits?.UpperControlLimit);
            })
            .ToList();

        var inspection = Domain.Inspections.Inspection.Create(request.CompanyId, request.Type, request.ReferenceId, characteristicInputs);

        await _repository.AddAsync(inspection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InspectionMapper.ToDto(inspection);
    }
}
