using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Queries.GetQualityHoldById;

public sealed class GetQualityHoldByIdQueryHandler : IRequestHandler<GetQualityHoldByIdQuery, QualityHoldDto>
{
    private readonly IQualityHoldRepository _repository;

    public GetQualityHoldByIdQueryHandler(IQualityHoldRepository repository) => _repository = repository;

    public async Task<QualityHoldDto> Handle(GetQualityHoldByIdQuery request, CancellationToken cancellationToken)
    {
        var hold = await _repository.GetByIdAsync(request.CompanyId, request.QualityHoldId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quality hold '{request.QualityHoldId}' was not found.");

        return QualityHoldMapper.ToDto(hold);
    }
}
