using FusionOS.Modules.Core.Application.Activity.Contracts;
using FusionOS.Modules.Core.Application.AuditLog.Contracts;
using FusionOS.Modules.Core.Application.Comments.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Activity.Queries.GetEntityActivityTimeline;

public sealed class GetEntityActivityTimelineQueryHandler : IRequestHandler<GetEntityActivityTimelineQuery, IReadOnlyList<ActivityTimelineEntryDto>>
{
    private readonly IAuditLogRepository _auditLog;
    private readonly ICommentRepository _comments;

    public GetEntityActivityTimelineQueryHandler(IAuditLogRepository auditLog, ICommentRepository comments)
    {
        _auditLog = auditLog;
        _comments = comments;
    }

    public async Task<IReadOnlyList<ActivityTimelineEntryDto>> Handle(GetEntityActivityTimelineQuery request, CancellationToken cancellationToken)
    {
        var auditEntries = await _auditLog.ListByEntityAsync(request.CompanyId, request.EntityType, request.EntityId, cancellationToken);
        var comments = await _comments.ListByEntityAsync(request.CompanyId, request.EntityType, request.EntityId, cancellationToken);

        var auditRows = auditEntries.Select(a => new ActivityTimelineEntryDto(
            a.Id, ActivityTimelineEntryDto.AuditEventKind, a.OccurredAt, a.ActorId, a.Action));

        var commentRows = comments.Select(c => new ActivityTimelineEntryDto(
            c.Id, ActivityTimelineEntryDto.CommentKind, c.CreatedAt, c.AuthorUserId, c.Body));

        // Both source lists are already individually chronological (oldest
        // first) — a single OrderBy over their concatenation interleaves them
        // correctly without needing a merge-sort, since .NET's OrderBy is a
        // stable sort.
        return auditRows.Concat(commentRows)
            .OrderBy(entry => entry.Timestamp)
            .ToList();
    }
}
