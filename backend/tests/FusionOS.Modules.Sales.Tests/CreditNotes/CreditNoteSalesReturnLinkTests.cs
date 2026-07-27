using FusionOS.Modules.Sales.Domain.CreditNotes;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.CreditNotes;

/// <summary>
/// Covers CreditNote.Create's new optional SalesReturnId parameter (2026-07-22 RMA
/// follow-up) — same "optional trailing parameter, existing callers unaffected" shape
/// as DispatchTests covers for Dispatch.Create's PickListId.
/// </summary>
public class CreditNoteSalesReturnLinkTests
{
    [Fact]
    public void Create_WithoutSalesReturnId_DefaultsToNull()
    {
        var creditNote = CreditNote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 2m, 10m) });

        creditNote.SalesReturnId.Should().BeNull();
    }

    [Fact]
    public void Create_WithSalesReturnId_SetsIt()
    {
        var salesReturnId = Guid.NewGuid();

        var creditNote = CreditNote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 2m, 10m) }, salesReturnId);

        creditNote.SalesReturnId.Should().Be(salesReturnId);
    }

    [Fact]
    public void Create_WithEmptySalesReturnId_Throws()
    {
        var act = () => CreditNote.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 2m, 10m) }, Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }
}
