using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Application.Comments.Queries.ListCommentsByEntity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Comments;

public class ListCommentsByEntityQueryHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesStraightToTheRepositorysListByEntity()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var expected = new List<CommentDto>
        {
            new(Guid.NewGuid(), "PurchaseOrder", entityId, "First", Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-10), null),
            new(Guid.NewGuid(), "PurchaseOrder", entityId, "Second", Guid.NewGuid(), DateTimeOffset.UtcNow, null),
        };
        var repository = Substitute.For<ICommentRepository>();
        repository.ListByEntityAsync(companyId, "PurchaseOrder", entityId, Arg.Any<CancellationToken>()).Returns(expected);
        var handler = new ListCommentsByEntityQueryHandler(repository);

        var result = await handler.Handle(new ListCommentsByEntityQuery(companyId, "PurchaseOrder", entityId), CancellationToken.None);

        result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }
}
