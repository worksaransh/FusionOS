using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Roles.Commands.CreateRole;
using FusionOS.Modules.Core.Domain.Identity;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Roles;

public class CreateRoleCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUniqueName_CreatesRoleWithNoPermissions()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        users.RoleNameExistsAsync(companyId, "Warehouse Clerk", Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateRoleCommandHandler(users, unitOfWork);

        var result = await handler.Handle(new CreateRoleCommand(companyId, "Warehouse Clerk"), CancellationToken.None);

        result.Name.Should().Be("Warehouse Clerk");
        result.IsSystemRole.Should().BeFalse();
        await users.Received(1).AddRoleAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ThrowsValidationException()
    {
        var users = Substitute.For<IUserRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        users.RoleNameExistsAsync(companyId, "Owner", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateRoleCommandHandler(users, unitOfWork);

        var act = () => handler.Handle(new CreateRoleCommand(companyId, "Owner"), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await users.DidNotReceive().AddRoleAsync(Arg.Any<Role>(), Arg.Any<CancellationToken>());
    }
}
