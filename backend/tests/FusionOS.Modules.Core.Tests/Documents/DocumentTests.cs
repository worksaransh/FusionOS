using FusionOS.Modules.Core.Domain.Documents;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Documents;

/// <summary>Covers Document (net-new generic file-attachment subsystem, 2026-07-21).</summary>
public class DocumentTests
{
    [Fact]
    public void Upload_WithValidArguments_SetsAllProperties()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var uploadedBy = Guid.NewGuid();
        var content = new byte[] { 1, 2, 3, 4 };

        var document = Document.Upload(companyId, "Invoice", entityId, "receipt.pdf", "application/pdf", content, uploadedBy);

        document.CompanyId.Should().Be(companyId);
        document.EntityType.Should().Be("Invoice");
        document.EntityId.Should().Be(entityId);
        document.FileName.Should().Be("receipt.pdf");
        document.ContentType.Should().Be("application/pdf");
        document.FileSizeBytes.Should().Be(content.Length);
        document.Content.Should().BeEquivalentTo(content);
        document.UploadedByUserId.Should().Be(uploadedBy);
    }

    [Fact]
    public void Upload_WithNoContentType_DefaultsToOctetStream()
    {
        var document = Document.Upload(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "file.bin", null, new byte[] { 1 }, Guid.NewGuid());

        document.ContentType.Should().Be("application/octet-stream");
    }

    [Fact]
    public void Upload_WithOversizedFile_Throws()
    {
        var oversized = new byte[Document.MaxFileSizeBytes + 1];

        var act = () => Document.Upload(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "huge.zip", "application/zip", oversized, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Upload_AtExactlyTheMaxSize_Succeeds()
    {
        var atLimit = new byte[Document.MaxFileSizeBytes];

        var document = Document.Upload(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "exact.zip", "application/zip", atLimit, Guid.NewGuid());

        document.FileSizeBytes.Should().Be(Document.MaxFileSizeBytes);
    }

    [Fact]
    public void Upload_WithEmptyFileName_Throws()
    {
        var act = () => Document.Upload(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "  ", "application/pdf", new byte[] { 1 }, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Upload_WithEmptyContent_Throws()
    {
        var act = () => Document.Upload(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "empty.txt", "text/plain", Array.Empty<byte>(), Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Upload_WithEmptyEntityType_Throws()
    {
        var act = () => Document.Upload(Guid.NewGuid(), " ", Guid.NewGuid(), "file.txt", "text/plain", new byte[] { 1 }, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Upload_WithEmptyEntityId_Throws()
    {
        var act = () => Document.Upload(Guid.NewGuid(), "Invoice", Guid.Empty, "file.txt", "text/plain", new byte[] { 1 }, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Upload_WithEmptyUploadedByUserId_Throws()
    {
        var act = () => Document.Upload(Guid.NewGuid(), "Invoice", Guid.NewGuid(), "file.txt", "text/plain", new byte[] { 1 }, Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
