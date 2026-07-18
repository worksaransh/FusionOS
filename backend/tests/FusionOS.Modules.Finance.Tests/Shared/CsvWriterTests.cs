using FusionOS.BuildingBlocks.Application.Csv;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Shared;

/// <summary>
/// Covers FusionOS.BuildingBlocks.Application.Csv.CsvWriter (Phase M6,
/// 2026-07-15). Lives in this test project rather than a dedicated
/// BuildingBlocks.Tests project — none exists yet, and Finance.Tests already
/// carries a transitive reference to BuildingBlocks.Application via
/// Finance.Application, the same transitivity reasoning already used to wire
/// CsvWriter into every module's Api controllers without a direct
/// ProjectReference.
/// </summary>
public class CsvWriterTests
{
    private sealed record SampleRow(Guid Id, string Name, decimal Amount, DateTimeOffset CreatedAt, string? Description);

    private sealed record RowWithCollection(Guid Id, string Name, IReadOnlyList<string> Tags);

    [Fact]
    public void Write_EmitsAHeaderRowFromPropertyNames()
    {
        var csv = CsvWriter.Write(Array.Empty<SampleRow>());

        csv.Should().Be("Id,Name,Amount,CreatedAt,Description\r\n");
    }

    [Fact]
    public void Write_EmitsOneDataRowPerItem()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var rows = new[] { new SampleRow(id, "Widget", 12.5m, createdAt, null) };

        var csv = CsvWriter.Write(rows);

        csv.Should().Contain($"{id},Widget,12.5,{createdAt:O},");
    }

    [Fact]
    public void Write_SkipsNonScalarProperties()
    {
        var rows = new[] { new RowWithCollection(Guid.NewGuid(), "Widget", new[] { "a", "b" }) };

        var csv = CsvWriter.Write(rows);
        var header = csv.Split("\r\n")[0];

        header.Should().Be("Id,Name");
        header.Should().NotContain("Tags");
    }

    [Fact]
    public void Write_QuotesFieldsContainingCommasOrQuotes()
    {
        var rows = new[] { new SampleRow(Guid.NewGuid(), "Acme, Inc.", 1m, DateTimeOffset.UtcNow, "Says \"hello\"") };

        var csv = CsvWriter.Write(rows);

        csv.Should().Contain("\"Acme, Inc.\"");
        csv.Should().Contain("\"Says \"\"hello\"\"\"");
    }

    [Fact]
    public void Write_RendersNullScalarAsAnEmptyField()
    {
        var rows = new[] { new SampleRow(Guid.NewGuid(), "Widget", 1m, DateTimeOffset.UtcNow, null) };

        var csv = CsvWriter.Write(rows);

        csv.TrimEnd('\r', '\n').Should().EndWith(",");
    }
}
