using FusionOS.Modules.Core.Application.Companies.Commands.CreateCompany;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Companies;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Companies;

public class CreateCompanyCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsCompanyAndReturnsDto()
    {
        var repository = Substitute.For<ICompanyRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCompanyCommandHandler(repository, unitOfWork);
        var command = new CreateCompanyCommand("Acme Trading", "Acme Trading Pvt Ltd", "INR", "GSTIN123");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Acme Trading");
        await repository.Received(1).AddAsync(Arg.Any<Company>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
