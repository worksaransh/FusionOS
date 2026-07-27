using FusionOS.Modules.Sales.Domain.PortalUsers;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.PortalUsers;

public class CustomerPortalUserTests
{
    [Fact]
    public void Create_WithValidArguments_IsActiveAndNormalizesEmail()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var portalUser = CustomerPortalUser.Create(companyId, customerId, "  Contact@Example.COM ", "hashed-value");

        portalUser.CompanyId.Should().Be(companyId);
        portalUser.CustomerId.Should().Be(customerId);
        portalUser.Email.Should().Be("contact@example.com");
        portalUser.PasswordHash.Should().Be("hashed-value");
        portalUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyCustomerId_Throws()
    {
        var act = () => CustomerPortalUser.Create(Guid.NewGuid(), Guid.Empty, "contact@example.com", "hash");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Create_WithInvalidEmail_Throws(string email)
    {
        var act = () => CustomerPortalUser.Create(Guid.NewGuid(), Guid.NewGuid(), email, "hash");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNoPasswordHash_Throws()
    {
        var act = () => CustomerPortalUser.Create(Guid.NewGuid(), Guid.NewGuid(), "contact@example.com", " ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_ThenActivate_TogglesIsActive()
    {
        var portalUser = CustomerPortalUser.Create(Guid.NewGuid(), Guid.NewGuid(), "contact@example.com", "hash");

        portalUser.Deactivate();
        portalUser.IsActive.Should().BeFalse();

        portalUser.Activate();
        portalUser.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChangePasswordHash_UpdatesHash()
    {
        var portalUser = CustomerPortalUser.Create(Guid.NewGuid(), Guid.NewGuid(), "contact@example.com", "hash");

        portalUser.ChangePasswordHash("new-hash");

        portalUser.PasswordHash.Should().Be("new-hash");
    }

    [Fact]
    public void ChangePasswordHash_WithBlankHash_Throws()
    {
        var portalUser = CustomerPortalUser.Create(Guid.NewGuid(), Guid.NewGuid(), "contact@example.com", "hash");

        var act = () => portalUser.ChangePasswordHash(" ");

        act.Should().Throw<ArgumentException>();
    }
}
