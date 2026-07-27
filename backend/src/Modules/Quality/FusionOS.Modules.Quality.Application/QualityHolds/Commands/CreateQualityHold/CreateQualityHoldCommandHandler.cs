using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Commands.CreateQualityHold;

public sealed class CreateQualityHoldCommandHandler : IRequestHandler<CreateQualityHoldCommand, QualityHoldDto>
{
    private readonly IQualityHoldRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQualityHoldCommandHandler(IQualityHoldRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QualityHoldDto> Handle(CreateQualityHoldCommand request, CancellationToken cancellationToken)
    {
        var hold = Domain.QualityHolds.QualityHold.Create(request.CompanyId, request.EntityType, request.EntityId, request.Reason, request.InspectionId);

        await _repository.AddAsync(hold, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QualityHoldMapper.ToDto(hold);
    }
}
