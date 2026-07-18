using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;

public sealed class CreateInspectionCommandHandler : IRequestHandler<CreateInspectionCommand, InspectionDto>
{
    private readonly IInspectionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInspectionCommandHandler(IInspectionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InspectionDto> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = Domain.Inspections.Inspection.Create(request.CompanyId, request.Type, request.ReferenceId, request.Characteristics);

        await _repository.AddAsync(inspection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return InspectionMapper.ToDto(inspection);
    }
}
