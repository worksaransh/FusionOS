using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Deviations.Queries.GetDeviationRequestById;

public sealed class GetDeviationRequestByIdQueryHandler : IRequestHandler<GetDeviationRequestByIdQuery, DeviationRequestDto>
{
    private readonly IDeviationRequestRepository _repository;

    public GetDeviationRequestByIdQueryHandler(IDeviationRequestRepository repository) => _repository = repository;

    public async Task<DeviationRequestDto> Handle(GetDeviationRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var deviationRequest = await _repository.GetByIdAsync(request.CompanyId, request.DeviationRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Deviation request '{request.DeviationRequestId}' was not found.");

        return DeviationRequestMapper.ToDto(deviationRequest);
    }
}
