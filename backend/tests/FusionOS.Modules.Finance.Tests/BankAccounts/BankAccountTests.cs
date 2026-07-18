using FluentAssertions;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.BankAccounts;

public class BankAccountTests
{
    [Fact]
    public void Create_NormalizesCode_ToUppercaseTrimmed()
    {
        var bankAccount = BankAccount.Create(Guid.NewGuid(), "  ops-checking  ", "Operating Checking", Guid.NewGuid(), "First National", "1234");

        bankAccount.Code.Should().Be("OPS-CHECKING");
        bankAccount.Name.Should().Be("Operating Checking");
        bankAccount.BankName.Should().Be("First National");
        bankAccount.AccountNumberLast4.Should().Be("1234");
        bankAccount.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithoutOptionalFields_LeavesThemNull()
    {
        var bankAccount = BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);

        bankAccount.BankName.Should().BeNull();
        bankAccount.AccountNumberLast4.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCode_Throws(string invalidCode)
    {
        var act = () => BankAccount.Create(Guid.NewGuid(), invalidCode, "Operating Checking", Guid.NewGuid(), null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_Throws(string invalidName)
    {
        var act = () => BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", invalidName, Guid.NewGuid(), null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyLinkedAccountId_Throws()
    {
        var act = () => BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.Empty, null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithAccountNumberLast4LongerThanFourChars_Throws()
    {
        var act = () => BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, "123456789");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesFields()
    {
        var bankAccount = BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);

        bankAccount.UpdateDetails("Operating Checking (Renamed)", "Second National", "5678");

        bankAccount.Name.Should().Be("Operating Checking (Renamed)");
        bankAccount.BankName.Should().Be("Second National");
        bankAccount.AccountNumberLast4.Should().Be("5678");
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var bankAccount = BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);

        var act = () => bankAccount.UpdateDetails(" ", null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_WithAccountNumberLast4LongerThanFourChars_Throws()
    {
        var bankAccount = BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);

        var act = () => bankAccount.UpdateDetails("Operating Checking", null, "123456789");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var bankAccount = BankAccount.Create(Guid.NewGuid(), "OPS-CHECKING", "Operating Checking", Guid.NewGuid(), null, null);

        bankAccount.Deactivate();

        bankAccount.IsActive.Should().BeFalse();
    }
}
