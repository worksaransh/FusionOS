using FusionOS.Modules.Core.Application.Documents.Contracts;
using FusionOS.Modules.Core.Application.Documents.Queries.DownloadDocument;
using FusionOS.Modules.Core.Domain.Documents;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Documents;

public class DownloadDocumentQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithExistingDocument_ReturnsItsBytesAndMetadata()
    {
        var companyId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var content = new byte[] { 9, 8, 7 };
        var document = Document.Upload(companyId, "Invoice", Guid.NewGuid(), "receipt.pdf", "application/pdf", content, Guid.NewGuid());

        var documents = Substitute.For<IDocumentRepository>();
        documents.GetByIdAsync(companyId, documentId, Arg.Any<CancellationToken>()).Returns(document);
        var handler = new DownloadDocumentQueryHandler(documents);

        var result = await handler.Handle(new DownloadDocumentQuery(companyId, documentId), CancellationToken.None);

        result.FileName.Should().Be("receipt.pdf");
        result.ContentType.Should().Be("application/pdf");
        result.Content.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task Handle_WithMissingDocument_Throws()
    {
        var documents = Substitute.For<IDocumentRepository>();
        documents.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Document?)null);
        var handler = new DownloadDocumentQueryHandler(documents);

        var act = () => handler.Handle(new DownloadDocumentQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
