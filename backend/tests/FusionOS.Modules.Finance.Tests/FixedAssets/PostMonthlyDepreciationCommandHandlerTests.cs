using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.PostMonthlyDepreciation;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class PostMonthlyDepreciationCommandHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (PostMonthlyDepreciationCommandHandler handler, IJournalEntryRepository ledger, FixedAsset asset, Guid expenseAccountId)
        BuildHandler(Guid companyId, Guid? accumulatedDepreciationAccountId, Guid? costCenterId = null, bool disposed = false)
    {
        var expenseAccountId = Guid.NewGuid();
        var asset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(),
            accumulatedDepreciationAccountId, costCenterId, AcquisitionDate, 24000m, 4000m, 60);
        if (disposed)
            asset.Dispose(AcquisitionDate.AddMonths(10));

        var fixedAssetRepository = Substitute.For<IFixedAssetRepository>();
        fixedAssetRepository.GetByIdAsync(companyId, asset.Id, Arg.Any<CancellationToken>()).Returns(asset);

        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var ledger = Substitute.For<IJournalEntryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new PostMonthlyDepreciationCommandHandler(fixedAssetRepository, accountRepository, ledger, unitOfWork);
        return (handler, ledger, asset, expenseAccountId);
    }

    [Fact]
    public async Task Handle_PostsBalancedPostedEntryForOneMonthOfDepreciation()
    {
        var companyId = Guid.NewGuid();
        var costCenterId = Guid.NewGuid();
        var (handler, _, asset, expenseAccountId) = BuildHandler(companyId, Guid.NewGuid(), costCenterId);
        var expectedMonthly = Math.Round((24000m - 4000m) / 60, 4, MidpointRounding.AwayFromZero);

        var result = await handler.Handle(
            new PostMonthlyDepreciationCommand(companyId, asset.Id, expenseAccountId, AcquisitionDate.AddMonths(1)),
            CancellationToken.None);

        result.Status.Should().Be("Posted");
        result.TotalDebit.Should().Be(expectedMonthly);
        result.TotalCredit.Should().Be(expectedMonthly);
        result.Lines.Should().HaveCount(2);
        result.Lines.Single(l => l.Debit > 0).CostCenterId.Should().Be(costCenterId);
    }

    [Fact]
    public async Task Handle_WhenAssetHasNoAccumulatedDepreciationAccount_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var (handler, _, asset, expenseAccountId) = BuildHandler(companyId, accumulatedDepreciationAccountId: null);

        var act = () => handler.Handle(
            new PostMonthlyDepreciationCommand(companyId, asset.Id, expenseAccountId, AcquisitionDate.AddMonths(1)),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenAssetDisposed_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var (handler, _, asset, expenseAccountId) = BuildHandler(companyId, Guid.NewGuid(), disposed: true);

        var act = () => handler.Handle(
            new PostMonthlyDepreciationCommand(companyId, asset.Id, expenseAccountId, AcquisitionDate.AddMonths(11)),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
