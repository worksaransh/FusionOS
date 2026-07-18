using FluentAssertions;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class FixedAssetTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidData_CreatesActiveNonDisposedAsset()
    {
        var assetAccountId = Guid.NewGuid();

        var fixedAsset = FixedAsset.Create(
            Guid.NewGuid(), "  fa-100  ", "Delivery Van #3", assetAccountId, null, null,
            AcquisitionDate, 24000m, 4000m, 60);

        fixedAsset.Code.Should().Be("FA-100");
        fixedAsset.Name.Should().Be("Delivery Van #3");
        fixedAsset.AssetAccountId.Should().Be(assetAccountId);
        fixedAsset.AccumulatedDepreciationAccountId.Should().BeNull();
        fixedAsset.CostCenterId.Should().BeNull();
        fixedAsset.AcquisitionCost.Should().Be(24000m);
        fixedAsset.SalvageValue.Should().Be(4000m);
        fixedAsset.UsefulLifeMonths.Should().Be(60);
        fixedAsset.IsDisposed.Should().BeFalse();
        fixedAsset.DisposedDate.Should().BeNull();
        fixedAsset.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidCode_Throws(string invalidCode)
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), invalidCode, "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithInvalidName_Throws(string invalidName)
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), "FA-100", invalidName, Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyAssetAccountId_Throws()
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.Empty, null, null, AcquisitionDate, 24000m, 4000m, 60);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Create_WithNonPositiveAcquisitionCost_Throws(decimal invalidCost)
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, invalidCost, 0m, 60);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeSalvageValue_Throws()
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, -1m, 60);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithSalvageValueEqualToAcquisitionCost_Throws()
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 24000m, 60);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithSalvageValueGreaterThanAcquisitionCost_Throws()
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 30000m, 60);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-12)]
    public void Create_WithNonPositiveUsefulLifeMonths_Throws(int invalidMonths)
    {
        var act = () => FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, invalidMonths);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesNameAndCostCenter()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var costCenterId = Guid.NewGuid();

        fixedAsset.UpdateDetails("Delivery Van #3 (repainted)", costCenterId);

        fixedAsset.Name.Should().Be("Delivery Van #3 (repainted)");
        fixedAsset.CostCenterId.Should().Be(costCenterId);
    }

    [Fact]
    public void UpdateDetails_DoesNotChangeFinancialFields()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);

        fixedAsset.UpdateDetails("Van", null);

        fixedAsset.AcquisitionCost.Should().Be(24000m);
        fixedAsset.SalvageValue.Should().Be(4000m);
        fixedAsset.UsefulLifeMonths.Should().Be(60);
    }

    [Fact]
    public void UpdateDetails_WithInvalidName_Throws()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);

        var act = () => fixedAsset.UpdateDetails(" ", null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Dispose_WithValidDate_SetsIsDisposedAndDisposedDate()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var disposedDate = AcquisitionDate.AddYears(3);

        fixedAsset.Dispose(disposedDate);

        fixedAsset.IsDisposed.Should().BeTrue();
        fixedAsset.DisposedDate.Should().Be(disposedDate);
    }

    [Fact]
    public void Dispose_WithDateBeforeAcquisitionDate_Throws()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);

        var act = () => fixedAsset.Dispose(AcquisitionDate.AddDays(-1));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Dispose_WhenAlreadyDisposed_Throws()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        fixedAsset.Dispose(AcquisitionDate.AddYears(1));

        var act = () => fixedAsset.Dispose(AcquisitionDate.AddYears(2));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);

        fixedAsset.Deactivate();

        fixedAsset.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_AndDispose_AreIndependent()
    {
        var fixedAsset = FixedAsset.Create(Guid.NewGuid(), "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);

        // Deactivating alone does not mark the asset disposed.
        fixedAsset.Deactivate();
        fixedAsset.IsDisposed.Should().BeFalse();

        // Disposing a fresh asset does not, by itself, deactivate it.
        var disposedOnly = FixedAsset.Create(Guid.NewGuid(), "FA-101", "Van 2", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        disposedOnly.Dispose(AcquisitionDate.AddYears(1));
        disposedOnly.IsActive.Should().BeTrue();
    }
}
