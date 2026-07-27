using FusionOS.Modules.Ai.Application.OcrJobs.Contracts;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Commands.SubmitOcrJob;

/// <summary>
/// Creates the OcrJob as Pending, then immediately calls <see cref="IOcrProvider"/>
/// synchronously in this same handler/request — the simplest honest approach available.
/// `docs/blueprint/12_AI_PLATFORM.md` §6 explicitly allows OCR a looser latency budget
/// than the standard API target and describes the real production shape as "asynchronous
/// (job submitted, result delivered via notification/event)"; that requires a background
/// job queue this codebase does not have (grep for `IHostedService`/`BackgroundService` —
/// the only precedent, <c>OutboxDispatcher&lt;T&gt;</c>, relays already-committed domain
/// events to Kafka, it doesn't run arbitrary long-running work against an external API).
/// Building a fake queue here would be scaffolding with nothing real underneath. The
/// natural production evolution, once a real <see cref="IOcrProvider"/> makes calls slow
/// or expensive enough to matter, is to split this into "submit" (Pending, enqueue) and a
/// worker that calls StartProcessing/CompleteWithExtraction/Fail out of band — the
/// aggregate's state machine already supports that split without any change.
/// </summary>
public sealed class SubmitOcrJobCommandHandler : IRequestHandler<SubmitOcrJobCommand, OcrJobDto>
{
    private readonly IOcrJobRepository _repository;
    private readonly IOcrProvider _ocrProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitOcrJobCommandHandler(IOcrJobRepository repository, IOcrProvider ocrProvider, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _ocrProvider = ocrProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<OcrJobDto> Handle(SubmitOcrJobCommand request, CancellationToken cancellationToken)
    {
        var job = Domain.OcrJobs.OcrJob.Submit(request.CompanyId, request.DocumentId);
        job.StartProcessing();

        var result = await _ocrProvider.ExtractAsync(request.DocumentId, cancellationToken);

        if (result.Succeeded)
            job.CompleteWithExtraction(result.ExtractedFieldsJson!);
        else
            job.Fail(result.FailureReason ?? "OCR extraction failed for an unspecified reason.");

        await _repository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OcrJobMapper.ToDto(job);
    }
}
