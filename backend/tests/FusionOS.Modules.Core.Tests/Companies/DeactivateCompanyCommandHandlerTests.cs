using FusionOS.Modules.Core.Application.Companies.Commands.DeactivateCompany;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Companies;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Companies;

public class DeactivateCompanyCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCallerOwnsCompany_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var company = Company.Create("Acme", "Acme Ltd", "INR");
        var repository = Substitute.For<ICompanyRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(company);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(companyId);
        var handler = new DeactivateCompanyCommandHandler(repository, unitOfWork, currentUser);
        var command = new DeactivateCompanyCommand(companyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCallerBelongsToDifferentCompany_ReturnsNull()
    {
        var repository = Substitute.For<ICompanyRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(Guid.NewGuid());
        var handler = new DeactivateCompanyCommandHandler(repository, unitOfWork, currentUser);
        var command = new DeactivateCompanyCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
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
        var handler = new DeactivateCompanyCommandHandler(repository, unitOfWork, currentUser);
        var command = new DeactivateCompanyCommand(companyId);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeNull();
    }
}
