using FluentAssertions;
using FusionOS.Modules.Core.Domain.Identity;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Roles;

public class RoleTests
{
    [Fact]
    public void Rename_OnACompanyRole_UpdatesTheName()
    {
        var role = Role.CreateCompanyRole(Guid.NewGuid(), "Warehouse Clerk");

        role.Rename("Senior Warehouse Clerk");

        role.Name.Should().Be("Senior Warehouse Clerk");
    }

    [Fact]
    public void Rename_TrimsWhitespace()
    {
        var role = Role.CreateCompanyRole(Guid.NewGuid(), "Warehouse Clerk");

        role.Rename("  Renamed  ");

        role.Name.Should().Be("Renamed");
    }

    [Fact]
    public void Rename_WithEmptyName_Throws()
    {
        var role = Role.CreateCompanyRole(Guid.NewGuid(), "Warehouse Clerk");

        var act = () => role.Rename("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rename_OnASystemRole_Throws()
    {
        var role = Role.CreateSystemRole("Owner");

        var act = () => role.Rename("Not Owner");

        act.Should().Throw<InvalidOperationException>();
    }
}
