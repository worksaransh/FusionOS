using System.Text.Json;
using FluentAssertions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Sales.Domain.Customers;
using FusionOS.SharedKernel.Events;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.IntegrationEvents.Consumers;

public class OpportunityWonConsumerTests
{
    private static string BuildPayload(Guid opportunityId, Guid companyId, string customerName, string customerCode, string? email) =>
        JsonSerializer.Serialize(new
        {
            OpportunityId = opportunityId,
            CompanyId = companyId,
            CustomerName = customerName,
            CustomerCode = customerCode,
            ContactEmail = email,
        });

    [Fact]
    public async Task HandleAsync_WhenCodeIsNew_CreatesCustomerAndMarksProcessed()
    {
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new OpportunityWonConsumer(customerRepository, processedEvents, unitOfWork);

        var eventId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var payload = BuildPayload(Guid.NewGuid(), companyId, "Acme Corp", "ACME", "sales@acme.com");

        await consumer.HandleAsync(eventId, companyId, payload, CancellationToken.None);

        await customerRepository.Received(1).AddAsync(
            Arg.Is<Customer>(c => c.Code == "ACME" && c.Name == "Acme Corp"),
            Arg.Any<CancellationToken>());
        processedEvents.Received(1).MarkProcessed(eventId, "OpportunityWon");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenCodeAlreadyExists_SkipsCreationButStillMarksProcessed()
    {
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new OpportunityWonConsumer(customerRepository, processedEvents, unitOfWork);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), BuildPayload(Guid.NewGuid(), Guid.NewGuid(), "Acme", "ACME", null), CancellationToken.None);

        await customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_DoesNothing()
    {
        var customerRepository = Substitute.For<ICustomerRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new OpportunityWonConsumer(customerRepository, processedEvents, unitOfWork);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), BuildPayload(Guid.NewGuid(), Guid.NewGuid(), "Acme", "ACME", null), CancellationToken.None);

        await customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
