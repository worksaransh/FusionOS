using FusionOS.Modules.Core.Application.Activity.Contracts;
using FusionOS.Modules.Core.Application.Activity.Queries.GetEntityActivityTimeline;
using FusionOS.Modules.Core.Application.AuditLog.Contracts;
using FusionOS.Modules.Core.Application.Comments.Contracts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Activity;

/// <summary>Covers the one thing this query exists for: correctly interleaving AuditLog entries and Comments into a single chronological feed.</summary>
public class GetEntityActivityTimelineQueryHandlerTests
{
    [Fact]
    public async Task Handle_InterleavesAuditEventsAndCommentsInChronologicalOrder()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var actor1 = Guid.NewGuid();
        var actor2 = Guid.NewGuid();
        var actor3 = Guid.NewGuid();

        var t0 = DateTimeOffset.Parse("2026-07-01T09:00:00Z");
        var t1 = DateTimeOffset.Parse("2026-07-01T10:00:00Z");
        var t2 = DateTimeOffset.Parse("2026-07-01T11:00:00Z");
        var t3 = DateTimeOffset.Parse("2026-07-01T12:00:00Z");

        var auditId = Guid.NewGuid();
        var auditEntries = new List<AuditLogEntryDto>
        {
            new(auditId, "PurchaseOrder", entityId, "Created", actor1, "actor1@example.com", companyId, null, t0, "corr-1"),
            new(Guid.NewGuid(), "PurchaseOrder", entityId, "Approved", actor2, "actor2@example.com", companyId, null, t2, "corr-2"),
        };

        var commentId = Guid.NewGuid();
        var comments = new List<CommentDto>
        {
            new(commentId, "PurchaseOrder", entityId, "Please expedite.", actor3, t1, null),
            new(Guid.NewGuid(), "PurchaseOrder", entityId, "Done, shipped.", actor2, t3, null),
        };

        var auditLog = Substitute.For<IAuditLogRepository>();
        auditLog.ListByEntityAsync(companyId, "PurchaseOrder", entityId, Arg.Any<CancellationToken>()).Returns(auditEntries);
        var commentRepository = Substitute.For<ICommentRepository>();
        commentRepository.ListByEntityAsync(companyId, "PurchaseOrder", entityId, Arg.Any<CancellationToken>()).Returns(comments);
        var handler = new GetEntityActivityTimelineQueryHandler(auditLog, commentRepository);

        var result = await handler.Handle(new GetEntityActivityTimelineQuery(companyId, "PurchaseOrder", entityId), CancellationToken.None);

        result.Should().HaveCount(4);
        result.Select(e => e.Timestamp).Should().BeInAscendingOrder();
        result.Select(e => e.Kind).Should().ContainInOrder(
            ActivityTimelineEntryDto.AuditEventKind,
            ActivityTimelineEntryDto.CommentKind,
            ActivityTimelineEntryDto.AuditEventKind,
            ActivityTimelineEntryDto.CommentKind);

        result[0].Id.Should().Be(auditId);
        result[0].ActorUserId.Should().Be(actor1);
        result[0].Description.Should().Be("Created");

        result[1].Id.Should().Be(commentId);
        result[1].ActorUserId.Should().Be(actor3);
        result[1].Description.Should().Be("Please expedite.");
    }

    [Fact]
    public async Task Handle_WithNoAuditEventsOrComments_ReturnsAnEmptyList()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var auditLog = Substitute.For<IAuditLogRepository>();
        auditLog.ListByEntityAsync(companyId, "PurchaseOrder", entityId, Arg.Any<CancellationToken>()).Returns(new List<AuditLogEntryDto>());
        var commentRepository = Substitute.For<ICommentRepository>();
        commentRepository.ListByEntityAsync(companyId, "PurchaseOrder", entityId, Arg.Any<CancellationToken>()).Returns(new List<CommentDto>());
        var handler = new GetEntityActivityTimelineQueryHandler(auditLog, commentRepository);

        var result = await handler.Handle(new GetEntityActivityTimelineQuery(companyId, "PurchaseOrder", entityId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
