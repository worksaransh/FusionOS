using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.AcceptRecommendation;

public sealed class AcceptRecommendationCommandHandler : IRequestHandler<AcceptRecommendationCommand, RecommendationDto>
{
    private readonly IRecommendationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptRecommendationCommandHandler(IRecommendationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecommendationDto> Handle(AcceptRecommendationCommand request, CancellationToken cancellationToken)
    {
        var recommendation = await _repository.GetByIdAsync(request.CompanyId, request.RecommendationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Recommendation '{request.RecommendationId}' was not found.");

        recommendation.Accept();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecommendationMapper.ToDto(recommendation);
    }
}
