using FluentAssertions;
using FusionOS.Modules.Finance.Domain.Accounts;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Accounts;

public class AccountTests
{
    [Fact]
    public void Create_NormalizesCode_ToUppercaseTrimmed()
    {
        var account = Account.Create(Guid.NewGuid(), "  1000  ", "Cash", AccountType.Asset, null);

        account.Code.Should().Be("1000");
        account.AccountType.Should().Be(AccountType.Asset);
        account.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyCode_Throws()
    {
        var act = () => Account.Create(Guid.NewGuid(), "  ", "Cash", AccountType.Asset, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var account = Account.Create(Guid.NewGuid(), "1000", "Cash", AccountType.Asset, null);

        account.Deactivate();

        account.IsActive.Should().BeFalse();
    }
}
