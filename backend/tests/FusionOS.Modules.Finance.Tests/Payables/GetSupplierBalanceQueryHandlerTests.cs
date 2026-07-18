using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Queries.GetSupplierBalance;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Payables;

public class GetSupplierBalanceQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheSummedBalanceFromTheRepository()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.SumAmountAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns(500.25m);
        var handler = new GetSupplierBalanceQueryHandler(repository);

        var result = await handler.Handle(new GetSupplierBalanceQuery(companyId, supplierId), CancellationToken.None);

        result.Balance.Should().Be(500.25m);
        result.SupplierId.Should().Be(supplierId);
    }
}
