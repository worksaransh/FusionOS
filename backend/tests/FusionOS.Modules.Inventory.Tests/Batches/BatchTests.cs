using FluentAssertions;
using FusionOS.Modules.Inventory.Domain.Batches;
using FusionOS.Modules.Inventory.Domain.Batches.Events;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Batches;

public class BatchTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();

    private static Batch New(decimal quantityReceived = 100m, DateTimeOffset? expiry = null) =>
        Batch.Create(Company, Product, "B-001", quantityReceived, expiry);

    [Fact]
    public void Create_SetsQuantityRemainingEqualToReceived_AndRaisesCreatedEvent()
    {
        var batch = New(100m);

        batch.QuantityReceived.Should().Be(100m);
        batch.QuantityRemaining.Should().Be(100m);
        batch.DomainEvents.Should().ContainSingle(e => e is BatchCreated);
    }

    [Fact]
    public void Create_WithZeroQuantity_Throws()
    {
        var act = () => Batch.Create(Company, Product, "B-001", 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyProductId_Throws()
    {
        var act = () => Batch.Create(Company, Guid.Empty, "B-001", 10m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithBlankBatchNumber_Throws()
    {
        var act = () => Batch.Create(Company, Product, "  ", 10m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Consume_ReducesQuantityRemaining()
    {
        var batch = New(100m);

        batch.Consume(40m);

        batch.QuantityRemaining.Should().Be(60m);
    }

    [Fact]
    public void Consume_MoreThanRemaining_Throws()
    {
        var batch = New(100m);
        batch.Consume(60m);

        var act = () => batch.Consume(50m);

        act.Should().Throw<InvalidOperationException>();
        batch.QuantityRemaining.Should().Be(40m); // unchanged by the rejected attempt
    }

    [Fact]
    public void Consume_ZeroOrNegativeQuantity_Throws()
    {
        var batch = New(100m);

        var act = () => batch.Consume(0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AdjustExpiry_UpdatesExpiryDate()
    {
        var batch = New();
        var newExpiry = DateTimeOffset.UtcNow.AddYears(1);

        batch.AdjustExpiry(newExpiry);

        batch.ExpiryDate.Should().Be(newExpiry);
    }

    [Fact]
    public void AdjustExpiry_WithNull_ClearsExpiryDate()
    {
        var batch = New(100m, DateTimeOffset.UtcNow.AddMonths(6));

        batch.AdjustExpiry(null);

        batch.ExpiryDate.Should().BeNull();
    }
}
