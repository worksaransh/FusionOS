using FluentAssertions;
using FusionOS.Modules.Crm.Domain.Accounts;
using FusionOS.Modules.Crm.Domain.Accounts.Events;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Accounts;

public class AccountTests
{
    private static readonly Guid Company = Guid.NewGuid();

    private static Account New() => Account.Create(Company, " Acme Corp ", "Manufacturing", "https://acme.example");

    [Fact]
    public void Create_NormalizesFields_AndRaisesEvent()
    {
        var account = New();

        account.Name.Should().Be("Acme Corp");
        account.Industry.Should().Be("Manufacturing");
        account.Website.Should().Be("https://acme.example");
        account.IsActive.Should().BeTrue();
        account.DomainEvents.Should().ContainSingle(e => e is AccountCreated);
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        var act = () => Account.Create(Company, "   ", null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_ChangesFields()
    {
        var account = New();

        account.UpdateDetails("Acme Holdings", "Logistics", null);

        account.Name.Should().Be("Acme Holdings");
        account.Industry.Should().Be("Logistics");
        account.Website.Should().BeNull();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var account = New();

        account.Deactivate();

        account.IsActive.Should().BeFalse();
    }
}
