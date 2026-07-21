using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Activity.Contracts;

namespace FusionOS.Modules.Core.Application.Activity.Queries.GetEntityActivityTimeline;

/// <summary>
/// The combined, read-only activity timeline for one (EntityType, EntityId) —
/// merges FusionOS.SharedKernel.Auditing's insert-only AuditLog trail with
/// user-authored Comments into a single chronological feed. Net-new
/// capability; doesn't replace either read side (AuditLogController and
/// CommentsController's own GET both still exist for callers that want just
/// one source).
/// </summary>
public sealed record GetEntityActivityTimelineQuery(Guid CompanyId, string EntityType, Guid EntityId)
    : IQuery<IReadOnlyList<ActivityTimelineEntryDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.activity.read" };
}
