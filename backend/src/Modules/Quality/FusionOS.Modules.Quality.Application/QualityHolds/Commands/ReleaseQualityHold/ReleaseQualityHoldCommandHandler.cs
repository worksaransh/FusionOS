using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Commands.ReleaseQualityHold;

public sealed class ReleaseQualityHoldCommandHandler : IRequestHandler<ReleaseQualityHoldCommand, QualityHoldDto>
{
    private readonly IQualityHoldRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReleaseQualityHoldCommandHandler(IQualityHoldRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QualityHoldDto> Handle(ReleaseQualityHoldCommand request, CancellationToken cancellationToken)
    {
        var hold = await _repository.GetByIdAsync(request.CompanyId, request.QualityHoldId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quality hold '{request.QualityHoldId}' was not found.");

        hold.Release(request.Note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QualityHoldMapper.ToDto(hold);
    }
}
