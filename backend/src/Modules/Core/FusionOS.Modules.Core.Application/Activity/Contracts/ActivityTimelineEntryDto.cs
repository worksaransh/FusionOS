namespace FusionOS.Modules.Core.Application.Activity.Contracts;

/// <summary>
/// One unified row in an entity's activity timeline — a discriminated union
/// of AuditLog's system-generated "what changed" events and Comment's
/// user-authored notes, merged and sorted chronologically by
/// GetEntityActivityTimelineQueryHandler. Description carries whichever of
/// the two source fields applies: an AuditLogEntryDto's Action for
/// Kind == "AuditEvent", or a CommentDto's Body for Kind == "Comment".
/// Id is the underlying AuditLog/Comment row's own id — for a "Comment" entry
/// this doubles as the CommentId a client needs to call
/// PUT/DELETE /api/v1/core/comments/{id}.
/// </summary>
public sealed record ActivityTimelineEntryDto(
    Guid Id,
    string Kind,
    DateTimeOffset Timestamp,
    Guid ActorUserId,
    string Description)
{
    public const string AuditEventKind = "AuditEvent";
    public const string CommentKind = "Comment";
}
