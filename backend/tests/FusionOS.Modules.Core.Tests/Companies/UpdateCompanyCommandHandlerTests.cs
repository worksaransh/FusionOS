using FusionOS.Modules.Core.Application.Companies.Commands.UpdateCompany;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Companies;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Companies;

public class UpdateCompanyCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCallerOwnsCompany_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var company = Company.Create("Old Name", "Old Legal Name", "INR", "GSTIN123");
        var repository = Substitute.For<ICompanyRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(company);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(companyId);
        var handler = new UpdateCompanyCommandHandler(repository, unitOfWork, currentUser);
        var command = new UpdateCompanyCommand(companyId, "New Name", "New Legal Name", "GSTIN999");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.LegalName.Should().Be("New Legal Name");
        result.TaxId.Should().Be("GSTIN999");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCallerBelongsToDifferentCompany_ReturnsNull()
    {
        var repository = Substitute.For<ICompanyRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(Guid.NewGuid());
        var handler = new UpdateCompanyCommandHandler(repository, unitOfWork, currentUser);
        var command = new UpdateCompanyCommand(Guid.NewGuid(), "New Name", "New Legal Name", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
        await repository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCompanyNotFound_ReturnsNull()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ICompanyRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((Company?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(companyId);
        var handler = new UpdateCompanyCommandHandler(repository, unitOfWork, currentUser);
        var command = new UpdateCompanyCommand(companyId, "New Name", "New Legal Name", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }
}
