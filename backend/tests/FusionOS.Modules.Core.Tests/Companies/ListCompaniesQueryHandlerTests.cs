using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Companies.Queries.ListCompanies;
using FusionOS.Modules.Core.Domain.Companies;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Companies;

public class ListCompaniesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyTheCallersOwnCompany()
    {
        var companyId = Guid.NewGuid();
        var company = Company.Create("Acme", "Acme Ltd", "INR");
        var repository = Substitute.For<ICompanyRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(company);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns(companyId);
        var handler = new ListCompaniesQueryHandler(repository, currentUser);

        var result = await handler.Handle(new ListCompaniesQuery(1, 25), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Name == "Acme");
    }

    [Fact]
    public async Task Handle_WhenCallerHasNoCompany_ReturnsEmpty()
    {
        var repository = Substitute.For<ICompanyRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.CompanyId.Returns((Guid?)null);
        var handler = new ListCompaniesQueryHandler(repository, currentUser);

        var result = await handler.Handle(new ListCompaniesQuery(1, 25), CancellationToken.None);

        result.TotalCount.Should().Be(0);
        result.Data.Should().BeEmpty();
    }
}
