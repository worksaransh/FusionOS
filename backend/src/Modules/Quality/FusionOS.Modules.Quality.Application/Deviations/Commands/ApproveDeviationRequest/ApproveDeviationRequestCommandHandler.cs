using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.ApproveDeviationRequest;

public sealed class ApproveDeviationRequestCommandHandler : IRequestHandler<ApproveDeviationRequestCommand, DeviationRequestDto>
{
    private readonly IDeviationRequestRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public ApproveDeviationRequestCommandHandler(IDeviationRequestRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviationRequestDto> Handle(ApproveDeviationRequestCommand request, CancellationToken cancellationToken)
    {
        var deviationRequest = await _repository.GetByIdAsync(request.CompanyId, request.DeviationRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Deviation request '{request.DeviationRequestId}' was not found.");

        deviationRequest.Approve(request.DecidedByUserId, request.Note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DeviationRequestMapper.ToDto(deviationRequest);
    }
}
