using FusionOS.Modules.Sales.Application.Customers.Commands.AssignPriceList;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using FusionOS.Modules.Sales.Domain.Customers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Customers;

public class AssignPriceListCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPriceListExists_AssignsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "Acme", "ACME-01");
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.GetByIdAsync(companyId, customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        var priceListRepository = Substitute.For<IPriceListRepository>();
        priceListRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignPriceListCommandHandler(customerRepository, priceListRepository, unitOfWork);
        var priceListId = Guid.NewGuid();

        var result = await handler.Handle(new AssignPriceListCommand(companyId, customer.Id, priceListId), CancellationToken.None);

        result.PriceListId.Should().Be(priceListId);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullPriceListId_ClearsAssignmentWithoutExistenceCheck()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "Acme", "ACME-01");
        customer.AssignPriceList(Guid.NewGuid());
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.GetByIdAsync(companyId, customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        var priceListRepository = Substitute.For<IPriceListRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignPriceListCommandHandler(customerRepository, priceListRepository, unitOfWork);

        var result = await handler.Handle(new AssignPriceListCommand(companyId, customer.Id, null), CancellationToken.None);

        result.PriceListId.Should().BeNull();
        await priceListRepository.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPriceListDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "Acme", "ACME-01");
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.GetByIdAsync(companyId, customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        var priceListRepository = Substitute.For<IPriceListRepository>();
        priceListRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignPriceListCommandHandler(customerRepository, priceListRepository, unitOfWork);

        var act = () => handler.Handle(new AssignPriceListCommand(companyId, customer.Id, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }
}
