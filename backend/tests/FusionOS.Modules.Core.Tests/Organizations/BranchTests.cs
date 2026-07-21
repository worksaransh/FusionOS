using FluentAssertions;
using FusionOS.Modules.Core.Domain.Organizations;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class BranchTests
{
    [Fact]
    public void Create_NormalizesCode_ToUppercaseTrimmed()
    {
        var branch = Branch.Create(Guid.NewGuid(), "  Head Office  ", "  hq-01  ");

        branch.Name.Should().Be("Head Office");
        branch.Code.Should().Be("HQ-01");
        branch.IsHeadOffice.Should().BeFalse();
        branch.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_Throws(string invalidName)
    {
        var act = () => Branch.Create(Guid.NewGuid(), invalidName, "HQ-01");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCode_Throws(string invalidCode)
    {
        var act = () => Branch.Create(Guid.NewGuid(), "Head Office", invalidCode);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidName_UpdatesNameAndIsHeadOffice()
    {
        var branch = Branch.Create(Guid.NewGuid(), "Head Office", "HQ-01");

        branch.UpdateDetails("Head Office (Renamed)", true);

        branch.Name.Should().Be("Head Office (Renamed)");
        branch.IsHeadOffice.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var branch = Branch.Create(Guid.NewGuid(), "Head Office", "HQ-01");

        var act = () => branch.UpdateDetails(" ", false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var branch = Branch.Create(Guid.NewGuid(), "Head Office", "HQ-01");

        branch.Deactivate();

        branch.IsActive.Should().BeFalse();
    }
}
