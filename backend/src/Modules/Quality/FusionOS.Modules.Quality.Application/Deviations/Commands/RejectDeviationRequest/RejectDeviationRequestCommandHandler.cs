using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.RejectDeviationRequest;

public sealed class RejectDeviationRequestCommandHandler : IRequestHandler<RejectDeviationRequestCommand, DeviationRequestDto>
{
    private readonly IDeviationRequestRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public RejectDeviationRequestCommandHandler(IDeviationRequestRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviationRequestDto> Handle(RejectDeviationRequestCommand request, CancellationToken cancellationToken)
    {
        var deviationRequest = await _repository.GetByIdAsync(request.CompanyId, request.DeviationRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Deviation request '{request.DeviationRequestId}' was not found.");

        deviationRequest.Reject(request.DecidedByUserId, request.Note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DeviationRequestMapper.ToDto(deviationRequest);
    }
}
