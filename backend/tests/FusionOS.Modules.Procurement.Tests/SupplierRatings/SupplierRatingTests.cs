using FusionOS.Modules.Procurement.Domain.SupplierRatings;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.SupplierRatings;

public class SupplierRatingTests
{
    [Fact]
    public void Create_WithValidArgs_SetsFieldsAndRaisesEvent()
    {
        var supplierId = Guid.NewGuid();
        var purchaseOrderId = Guid.NewGuid();
        var ratedByUserId = Guid.NewGuid();

        var rating = SupplierRating.Create(Guid.NewGuid(), supplierId, purchaseOrderId, 5, 4, ratedByUserId, "Great supplier.");

        rating.SupplierId.Should().Be(supplierId);
        rating.PurchaseOrderId.Should().Be(purchaseOrderId);
        rating.QualityScore.Should().Be(5);
        rating.DeliveryScore.Should().Be(4);
        rating.Comments.Should().Be("Great supplier.");
        rating.RatedByUserId.Should().Be(ratedByUserId);
        rating.DomainEvents.Should().ContainSingle(e => e is Events.SupplierRatingRecorded);
    }

    [Fact]
    public void Create_WithoutPurchaseOrder_LeavesPurchaseOrderIdNull()
    {
        var rating = SupplierRating.Create(Guid.NewGuid(), Guid.NewGuid(), null, 3, 3, Guid.NewGuid());

        rating.PurchaseOrderId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptySupplierId_Throws()
    {
        var act = () => SupplierRating.Create(Guid.NewGuid(), Guid.Empty, null, 3, 3, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyRatedByUserId_Throws()
    {
        var act = () => SupplierRating.Create(Guid.NewGuid(), Guid.NewGuid(), null, 3, 3, Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithOutOfRangeQualityScore_Throws(int qualityScore)
    {
        var act = () => SupplierRating.Create(Guid.NewGuid(), Guid.NewGuid(), null, qualityScore, 3, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Create_WithOutOfRangeDeliveryScore_Throws(int deliveryScore)
    {
        var act = () => SupplierRating.Create(Guid.NewGuid(), Guid.NewGuid(), null, 3, deliveryScore, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithBlankComments_LeavesCommentsNull()
    {
        var rating = SupplierRating.Create(Guid.NewGuid(), Guid.NewGuid(), null, 3, 3, Guid.NewGuid(), "   ");

        rating.Comments.Should().BeNull();
    }
}
