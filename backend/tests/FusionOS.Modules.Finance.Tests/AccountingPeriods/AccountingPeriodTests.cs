using FluentAssertions;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.AccountingPeriods;

public class AccountingPeriodTests
{
    [Fact]
    public void Create_WithValidRange_IsOpen()
    {
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);

        var period = AccountingPeriod.Create(Guid.NewGuid(), start, end);

        period.PeriodStart.Should().Be(start);
        period.PeriodEnd.Should().Be(end);
        period.Status.Should().Be(AccountingPeriodStatus.Open);
    }

    [Fact]
    public void Create_WithEndNotAfterStart_Throws()
    {
        var start = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var act = () => AccountingPeriod.Create(Guid.NewGuid(), start, end);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Covers_DateInsideRange_ReturnsTrue()
    {
        var period = AccountingPeriod.Create(Guid.NewGuid(), new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero));

        period.Covers(new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero)).Should().BeTrue();
        period.Covers(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)).Should().BeFalse();
    }

    [Theory]
    [InlineData("2026-01-15", "2026-02-15", true)]  // overlaps mid-range
    [InlineData("2025-12-01", "2026-01-01", true)]  // overlaps at the boundary (PeriodStart)
    [InlineData("2026-01-31", "2026-02-15", true)]  // overlaps at the boundary (PeriodEnd)
    [InlineData("2026-02-01", "2026-02-28", false)] // entirely after
    [InlineData("2025-11-01", "2025-12-31", false)] // entirely before
    public void OverlapsWith_VariousRanges_MatchesExpectation(string otherStart, string otherEnd, bool expectedOverlap)
    {
        var period = AccountingPeriod.Create(Guid.NewGuid(), new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero));

        // DateTimeOffset.Parse alone assumes the LOCAL machine's UTC offset, not zero —
        // on a non-UTC machine (e.g. IST, +5:30) that silently shifts these instants away
        // from the UTC-zero dates AccountingPeriod.Create uses above, flipping the exact
        // boundary cases this test exists to cover. Force zero offset explicitly instead.
        var overlaps = period.OverlapsWith(
            new DateTimeOffset(DateTime.Parse(otherStart), TimeSpan.Zero),
            new DateTimeOffset(DateTime.Parse(otherEnd), TimeSpan.Zero));

        overlaps.Should().Be(expectedOverlap);
    }

    [Fact]
    public void Close_WhenOpen_SetsClosed()
    {
        var period = AccountingPeriod.Create(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));

        period.Close();

        period.Status.Should().Be(AccountingPeriodStatus.Closed);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_Throws()
    {
        var period = AccountingPeriod.Create(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        period.Close();

        var act = () => period.Close();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reopen_WhenClosed_SetsOpen()
    {
        var period = AccountingPeriod.Create(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));
        period.Close();

        period.Reopen();

        period.Status.Should().Be(AccountingPeriodStatus.Open);
    }

    [Fact]
    public void Reopen_WhenAlreadyOpen_Throws()
    {
        var period = AccountingPeriod.Create(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(30));

        var act = () => period.Reopen();

        act.Should().Throw<InvalidOperationException>();
    }
}
