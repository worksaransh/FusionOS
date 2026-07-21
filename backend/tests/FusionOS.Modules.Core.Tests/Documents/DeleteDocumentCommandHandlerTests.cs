using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Documents.Commands.DeleteDocument;
using FusionOS.Modules.Core.Application.Documents.Contracts;
using FusionOS.Modules.Core.Domain.Documents;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Documents;

public class DeleteDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingDocument_SoftDeletesAndSaves()
    {
        var companyId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var actingUser = Guid.NewGuid();
        var document = Document.Upload(companyId, "Invoice", Guid.NewGuid(), "receipt.pdf", "application/pdf", new byte[] { 1 }, Guid.NewGuid());

        var documents = Substitute.For<IDocumentRepository>();
        documents.GetByIdAsync(companyId, documentId, Arg.Any<CancellationToken>()).Returns(document);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(actingUser);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteDocumentCommandHandler(documents, currentUser, unitOfWork);

        await handler.Handle(new DeleteDocumentCommand(companyId, documentId), CancellationToken.None);

        document.IsDeleted.Should().BeTrue();
        document.DeletedBy.Should().Be(actingUser);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMissingDocument_Throws()
    {
        var documents = Substitute.For<IDocumentRepository>();
        documents.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Document?)null);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteDocumentCommandHandler(documents, currentUser, unitOfWork);

        var act = () => handler.Handle(new DeleteDocumentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_Throws()
    {
        var document = Document.Upload(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "receipt.pdf", "application/pdf", new byte[] { 1 }, Guid.NewGuid());
        var documents = Substitute.For<IDocumentRepository>();
        documents.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(document);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns((Guid?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteDocumentCommandHandler(documents, currentUser, unitOfWork);

        var act = () => handler.Handle(new DeleteDocumentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
