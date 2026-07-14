using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Companies.Events;

public sealed record CompanyCreated(Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
