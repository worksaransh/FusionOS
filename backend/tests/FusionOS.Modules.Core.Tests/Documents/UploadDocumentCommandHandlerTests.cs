using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Documents.Commands.UploadDocument;
using FusionOS.Modules.Core.Application.Documents.Contracts;
using FusionOS.Modules.Core.Domain.Documents;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Documents;

public class UploadDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_PersistsDocumentAndReturnsMetadataOnly()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var uploadedBy = Guid.NewGuid();

        var documents = Substitute.For<IDocumentRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(uploadedBy);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UploadDocumentCommandHandler(documents, currentUser, unitOfWork);

        var command = new UploadDocumentCommand(companyId, "Invoice", entityId, "receipt.pdf", "application/pdf", new byte[] { 1, 2, 3 });

        var result = await handler.Handle(command, CancellationToken.None);

        result.EntityType.Should().Be("Invoice");
        result.EntityId.Should().Be(entityId);
        result.FileName.Should().Be("receipt.pdf");
        result.FileSizeBytes.Should().Be(3);
        result.UploadedByUserId.Should().Be(uploadedBy);
        await documents.Received(1).AddAsync(Arg.Is<Document>(d => d.EntityType == "Invoice" && d.EntityId == entityId), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_Throws()
    {
        var documents = Substitute.For<IDocumentRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns((Guid?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UploadDocumentCommandHandler(documents, currentUser, unitOfWork);

        var command = new UploadDocumentCommand(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "receipt.pdf", "application/pdf", new byte[] { 1 });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WithOversizedFile_Throws()
    {
        var documents = Substitute.For<IDocumentRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UploadDocumentCommandHandler(documents, currentUser, unitOfWork);

        var command = new UploadDocumentCommand(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "huge.zip", "application/zip", new byte[Document.MaxFileSizeBytes + 1]);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
