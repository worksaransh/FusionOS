using FusionOS.SharedKernel;

namespace FusionOS.Modules.Ai.Domain.OcrJobs.Events;

public sealed record OcrJobSubmitted(Guid OcrJobId, Guid CompanyId, Guid DocumentId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
