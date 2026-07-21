using FluentAssertions;
using FusionOS.Modules.Core.Domain.Organizations;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class DepartmentTests
{
    [Fact]
    public void Create_NormalizesCode_ToUppercaseTrimmed()
    {
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();

        var department = Department.Create(companyId, branchId, "  Engineering  ", "  eng-01  ");

        department.Name.Should().Be("Engineering");
        department.Code.Should().Be("ENG-01");
        department.BranchId.Should().Be(branchId);
        department.ParentDepartmentId.Should().BeNull();
        department.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_Throws(string invalidName)
    {
        var act = () => Department.Create(Guid.NewGuid(), null, invalidName, "ENG-01");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithParentDepartment_SetsParentDepartmentId()
    {
        var parentId = Guid.NewGuid();

        var department = Department.Create(Guid.NewGuid(), null, "Sub-team", "SUB-01", parentId);

        department.ParentDepartmentId.Should().Be(parentId);
    }

    [Fact]
    public void UpdateDetails_WithValidName_UpdatesNameBranchAndParent()
    {
        var department = Department.Create(Guid.NewGuid(), null, "Engineering", "ENG-01");
        var newBranchId = Guid.NewGuid();
        var newParentId = Guid.NewGuid();

        department.UpdateDetails("Engineering (Renamed)", newBranchId, newParentId);

        department.Name.Should().Be("Engineering (Renamed)");
        department.BranchId.Should().Be(newBranchId);
        department.ParentDepartmentId.Should().Be(newParentId);
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_Throws()
    {
        var department = Department.Create(Guid.NewGuid(), null, "Engineering", "ENG-01");

        var act = () => department.UpdateDetails(" ", null, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var department = Department.Create(Guid.NewGuid(), null, "Engineering", "ENG-01");

        department.Deactivate();

        department.IsActive.Should().BeFalse();
    }
}
