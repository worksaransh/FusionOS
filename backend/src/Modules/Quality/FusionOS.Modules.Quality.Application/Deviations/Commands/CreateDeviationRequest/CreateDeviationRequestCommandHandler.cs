using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.CreateDeviationRequest;

public sealed class CreateDeviationRequestCommandHandler : IRequestHandler<CreateDeviationRequestCommand, DeviationRequestDto>
{
    private readonly IDeviationRequestRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public CreateDeviationRequestCommandHandler(IDeviationRequestRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviationRequestDto> Handle(CreateDeviationRequestCommand request, CancellationToken cancellationToken)
    {
        var deviationRequest = Domain.Deviations.DeviationRequest.Create(request.CompanyId, request.InspectionId, request.Reason, request.RequestedByUserId);

        await _repository.AddAsync(deviationRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DeviationRequestMapper.ToDto(deviationRequest);
    }
}
