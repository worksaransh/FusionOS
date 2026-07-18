using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Queries.GetCustomerBalance;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Receivables;

public class GetCustomerBalanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheSummedBalanceFromTheRepository()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.SumAmountAsync(companyId, customerId, Arg.Any<CancellationToken>()).Returns(500.25m);
        var handler = new GetCustomerBalanceQueryHandler(repository);

        var result = await handler.Handle(new GetCustomerBalanceQuery(companyId, customerId), CancellationToken.None);

        result.Balance.Should().Be(500.25m);
        result.CustomerId.Should().Be(customerId);
    }
}
