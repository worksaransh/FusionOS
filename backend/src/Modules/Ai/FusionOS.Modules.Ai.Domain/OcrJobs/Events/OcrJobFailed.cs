using FusionOS.SharedKernel;

namespace FusionOS.Modules.Ai.Domain.OcrJobs.Events;

public sealed record OcrJobFailed(Guid OcrJobId, Guid CompanyId, Guid DocumentId, string FailureReason) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
