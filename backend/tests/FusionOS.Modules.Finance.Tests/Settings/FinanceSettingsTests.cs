using FusionOS.Modules.Finance.Domain.Settings;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Settings;

public class FinanceSettingsTests
{
    [Fact]
    public void CreateDefault_StartsWithAllAccountsUnset()
    {
        var companyId = Guid.NewGuid();

        var settings = FinanceSettings.CreateDefault(companyId);

        settings.CompanyId.Should().Be(companyId);
        settings.DefaultArAccountId.Should().BeNull();
        settings.DefaultSalesRevenueAccountId.Should().BeNull();
        settings.DefaultApAccountId.Should().BeNull();
        settings.DefaultPurchaseExpenseAccountId.Should().BeNull();
    }

    [Fact]
    public void CreateDefault_WithEmptyCompanyId_Throws()
    {
        var act = () => FinanceSettings.CreateDefault(Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ConfigureAccounts_SetsAllFourFields()
    {
        var settings = FinanceSettings.CreateDefault(Guid.NewGuid());
        var arAccountId = Guid.NewGuid();
        var revenueAccountId = Guid.NewGuid();
        var apAccountId = Guid.NewGuid();
        var expenseAccountId = Guid.NewGuid();

        settings.ConfigureAccounts(arAccountId, revenueAccountId, apAccountId, expenseAccountId);

        settings.DefaultArAccountId.Should().Be(arAccountId);
        settings.DefaultSalesRevenueAccountId.Should().Be(revenueAccountId);
        settings.DefaultApAccountId.Should().Be(apAccountId);
        settings.DefaultPurchaseExpenseAccountId.Should().Be(expenseAccountId);
    }

    [Fact]
    public void ConfigureAccounts_WithPartialInputs_LeavesUnsuppliedFieldsNull()
    {
        var settings = FinanceSettings.CreateDefault(Guid.NewGuid());
        var arAccountId = Guid.NewGuid();

        settings.ConfigureAccounts(arAccountId, null, null, null);

        settings.DefaultArAccountId.Should().Be(arAccountId);
        settings.DefaultSalesRevenueAccountId.Should().BeNull();
    }
}
