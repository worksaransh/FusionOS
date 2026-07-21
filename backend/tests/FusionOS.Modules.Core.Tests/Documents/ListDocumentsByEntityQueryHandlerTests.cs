using FusionOS.Modules.Core.Application.Documents.Contracts;
using FusionOS.Modules.Core.Application.Documents.Queries.ListDocumentsByEntity;
using FusionOS.Modules.Core.Domain.Documents;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Documents;

public class ListDocumentsByEntityQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMetadataOnlyDtosForTheGivenEntity()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var document = Document.Upload(companyId, "Invoice", entityId, "receipt.pdf", "application/pdf", new byte[] { 1, 2, 3 }, Guid.NewGuid());

        var documents = Substitute.For<IDocumentRepository>();
        documents.ListByEntityAsync(companyId, "Invoice", entityId, 1, 25, Arg.Any<CancellationToken>())
            .Returns(new[] { (document.Id, document.EntityType, document.EntityId, document.FileName, document.ContentType, document.FileSizeBytes, document.UploadedByUserId, document.UploadedAt) });
        documents.CountByEntityAsync(companyId, "Invoice", entityId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListDocumentsByEntityQueryHandler(documents);

        var result = await handler.Handle(new ListDocumentsByEntityQuery(companyId, "Invoice", entityId, 1, 25), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle();
        result.Data[0].FileName.Should().Be("receipt.pdf");
        result.Data[0].FileSizeBytes.Should().Be(3);
    }
}
