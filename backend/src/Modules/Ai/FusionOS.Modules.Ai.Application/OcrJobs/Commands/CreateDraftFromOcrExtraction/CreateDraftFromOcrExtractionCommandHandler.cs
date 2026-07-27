using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Domain.OcrJobs;
using FluentValidation.Results;
using MediatR;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Commands.CreateDraftFromOcrExtraction;

/// <summary>
/// See <see cref="CreateDraftFromOcrExtractionCommand"/>'s doc comment for the full
/// reasoning: this only performs the "link the extraction to a draft record" bookkeeping
/// step, never a real Finance/Procurement draft construction, and only ever proceeds past
/// a Completed OcrJob — which, with today's <see cref="UnavailableOcrProvider"/>, never
/// happens through the real submit pipeline. The defensive <see cref="ValidationException"/>
/// below is intentional: it surfaces that limitation instead of silently no-oping.
/// </summary>
public sealed class CreateDraftFromOcrExtractionCommandHandler : IRequestHandler<CreateDraftFromOcrExtractionCommand, OcrJobDto>
{
    private readonly IOcrJobRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDraftFromOcrExtractionCommandHandler(IOcrJobRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OcrJobDto> Handle(CreateDraftFromOcrExtractionCommand request, CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.CompanyId, request.OcrJobId, cancellationToken)
            ?? throw new KeyNotFoundException($"OCR job '{request.OcrJobId}' was not found.");

        if (job.Status != OcrJobStatus.Completed)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(request.OcrJobId),
                    $"OCR job '{request.OcrJobId}' is {job.Status}, not Completed — a draft record can only be created from " +
                    "a completed extraction. No real OCR provider is configured in this environment (see UnavailableOcrProvider), " +
                    "so a job will only ever reach Completed once a real IOcrProvider is wired up in AiModule.cs."),
            });
        }

        // The real "parse ExtractedFieldsJson into a Finance/Procurement draft bill" step
        // is deliberately not implemented here — see this command's class doc comment.
        // What's real: the extraction payload genuinely exists on the job (job.ExtractedFieldsJson)
        // and the bookkeeping link below genuinely records where its draft record ended up.
        job.LinkDraftRecord(request.TargetReferenceType, request.TargetReferenceId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OcrJobMapper.ToDto(job);
    }
}
