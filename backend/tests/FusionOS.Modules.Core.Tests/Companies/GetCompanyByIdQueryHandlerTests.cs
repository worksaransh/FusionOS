using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Companies.Queries.GetCompanyById;
using FusionOS.Modules.Core.Domain.Companies;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Companies;

public class GetCompanyByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenCallerOwnsCompany_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var company = Company.Create("Acme", "Acme Ltd", "INR");
        var repository = Substitute.For<ICompanyRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(company);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(companyId);
        var handler = new GetCompanyByIdQueryHandler(repository, currentUser);

        var result = await handler.Handle(new GetCompanyByIdQuery(companyId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Acme");
    }

    [Fact]
    public async Task Handle_WhenIdBelongsToDifferentCompany_ReturnsNull()
    {
        var repository = Substitute.For<ICompanyRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(Guid.NewGuid());
        var handler = new GetCompanyByIdQueryHandler(repository, currentUser);

        var result = await handler.Handle(new GetCompanyByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenCallerHasNoCompany_ReturnsNull()
    {
        var repository = Substitute.For<ICompanyRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns((Guid?)null);
        var handler = new GetCompanyByIdQueryHandler(repository, currentUser);

        var result = await handler.Handle(new GetCompanyByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
