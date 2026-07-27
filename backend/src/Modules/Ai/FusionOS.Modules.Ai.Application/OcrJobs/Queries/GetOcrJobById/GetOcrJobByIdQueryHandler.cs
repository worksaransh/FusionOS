using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Queries.GetOcrJobById;

public sealed class GetOcrJobByIdQueryHandler : IRequestHandler<GetOcrJobByIdQuery, OcrJobDto>
{
    private readonly IOcrJobRepository _repository;

    public GetOcrJobByIdQueryHandler(IOcrJobRepository repository) => _repository = repository;

    public async Task<OcrJobDto> Handle(GetOcrJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.CompanyId, request.OcrJobId, cancellationToken)
            ?? throw new KeyNotFoundException($"OCR job '{request.OcrJobId}' was not found.");

        return OcrJobMapper.ToDto(job);
    }
}
