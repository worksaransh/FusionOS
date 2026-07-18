using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Recommendations.Commands.RecordRecommendation;

public sealed class RecordRecommendationCommandHandler : IRequestHandler<RecordRecommendationCommand, RecommendationDto>
{
    private readonly IRecommendationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordRecommendationCommandHandler(IRecommendationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RecommendationDto> Handle(RecordRecommendationCommand request, CancellationToken cancellationToken)
    {
        var recommendation = Domain.Recommendations.Recommendation.Create(
            request.CompanyId, request.Type, request.ReferenceId, request.Summary, request.ModelVersion);

        await _repository.AddAsync(recommendation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RecommendationMapper.ToDto(recommendation);
    }
}
