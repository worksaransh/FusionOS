using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.PortalUsers.Commands.CreateCustomerPortalUser;
using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.PortalUsers;

public class CreateCustomerPortalUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerExistsAndEmailUnused_CreatesPortalUser()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(companyId, customerId, Arg.Any<CancellationToken>()).Returns(true);

        var portalUserRepository = Substitute.For<ICustomerPortalUserRepository>();
        portalUserRepository.ExistsForCustomerAsync(customerId, "contact@example.com", Arg.Any<CancellationToken>()).Returns(false);

        var hasher = Substitute.For<IPortalPasswordHasher>();
        hasher.Hash("plain-password").Returns("hashed-password");

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCustomerPortalUserCommandHandler(portalUserRepository, customerRepository, hasher, unitOfWork);

        var result = await handler.Handle(
            new CreateCustomerPortalUserCommand(companyId, customerId, "contact@example.com", "plain-password"),
            CancellationToken.None);

        result.CustomerId.Should().Be(customerId);
        result.Email.Should().Be("contact@example.com");
        await portalUserRepository.Received(1).AddAsync(
            Arg.Is<Domain.PortalUsers.CustomerPortalUser>(u => u.PasswordHash == "hashed-password" && u.CustomerId == customerId),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(companyId, customerId, Arg.Any<CancellationToken>()).Returns(false);

        var portalUserRepository = Substitute.For<ICustomerPortalUserRepository>();
        var hasher = Substitute.For<IPortalPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCustomerPortalUserCommandHandler(portalUserRepository, customerRepository, hasher, unitOfWork);

        var act = () => handler.Handle(
            new CreateCustomerPortalUserCommand(companyId, customerId, "contact@example.com", "plain-password"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyUsedForThisCustomer_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(companyId, customerId, Arg.Any<CancellationToken>()).Returns(true);

        var portalUserRepository = Substitute.For<ICustomerPortalUserRepository>();
        portalUserRepository.ExistsForCustomerAsync(customerId, "contact@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var hasher = Substitute.For<IPortalPasswordHasher>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCustomerPortalUserCommandHandler(portalUserRepository, customerRepository, hasher, unitOfWork);

        var act = () => handler.Handle(
            new CreateCustomerPortalUserCommand(companyId, customerId, "contact@example.com", "plain-password"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
