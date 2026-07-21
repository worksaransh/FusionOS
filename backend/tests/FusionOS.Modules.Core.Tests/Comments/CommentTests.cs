using FusionOS.Modules.Core.Domain.Comments;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Comments;

/// <summary>
/// Covers Comment's own shape invariants. Author-only edit enforcement and
/// the author-or-moderator delete override are deliberately NOT domain
/// concerns here — they live in UpdateCommentCommandHandler/
/// DeleteCommentCommandHandler (see Comment's doc comment for why) and are
/// covered by CommentTests's sibling handler test classes instead.
/// </summary>
public class CommentTests
{
    [Fact]
    public void Post_WithValidInputs_CreatesAPendingComment()
    {
        var companyId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var comment = Comment.Post(companyId, "PurchaseOrder", entityId, authorId, "Looks good to me.");

        comment.CompanyId.Should().Be(companyId);
        comment.EntityType.Should().Be("PurchaseOrder");
        comment.EntityId.Should().Be(entityId);
        comment.AuthorUserId.Should().Be(authorId);
        comment.Body.Should().Be("Looks good to me.");
    }

    [Fact]
    public void Post_TrimsSurroundingWhitespaceFromBody()
    {
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), "  padded  ");

        comment.Body.Should().Be("padded");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Post_WithNoBody_Throws(string? body)
    {
        var act = () => Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), body!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Post_WithBodyOverMaxLength_Throws()
    {
        var tooLong = new string('x', Comment.MaxBodyLength + 1);

        var act = () => Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), tooLong);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Post_AtExactlyMaxLength_Succeeds()
    {
        var exactlyMax = new string('x', Comment.MaxBodyLength);

        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), exactlyMax);

        comment.Body.Should().HaveLength(Comment.MaxBodyLength);
    }

    [Fact]
    public void Post_WithEmptyEntityType_Throws()
    {
        var act = () => Comment.Post(Guid.NewGuid(), "  ", Guid.NewGuid(), Guid.NewGuid(), "Body");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Post_WithEmptyEntityId_Throws()
    {
        var act = () => Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.Empty, Guid.NewGuid(), "Body");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateBody_WithAValidNewBody_ReplacesTheBody()
    {
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), "Original");

        comment.UpdateBody("Corrected");

        comment.Body.Should().Be("Corrected");
    }

    [Fact]
    public void UpdateBody_WithNoBody_ThrowsAndLeavesOriginalBodyUnchanged()
    {
        var comment = Comment.Post(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), "Original");

        var act = () => comment.UpdateBody("   ");

        act.Should().Throw<ArgumentException>();
        comment.Body.Should().Be("Original");
    }
}
