using FluentAssertions;
using FusionOS.Modules.Crm.Application.Campaigns.Commands.AddCampaignMember;
using FusionOS.Modules.Crm.Application.Campaigns.Commands.RemoveCampaignMember;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Domain.Campaigns;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Campaigns;

/// <summary>
/// AddCampaignMemberCommandHandler and RemoveCampaignMemberCommandHandler had zero test
/// coverage. AddCampaignMember's one piece of real logic is idempotency — re-adding an
/// already-enrolled member returns the existing membership rather than creating (or
/// erroring on) a duplicate — easy to accidentally regress into either a duplicate insert
/// or an unwanted exception.
/// </summary>
public class CampaignMembershipCommandHandlerTests
{
    private static readonly Guid Company = Guid.NewGuid();

    [Fact]
    public async Task AddCampaignMember_WhenCampaignMissing_ThrowsKeyNotFound()
    {
        var campaignRepository = Substitute.For<ICampaignRepository>();
        campaignRepository.ExistsAsync(Company, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var membershipRepository = Substitute.For<ICampaignMembershipRepository>();
        var handler = new AddCampaignMemberCommandHandler(campaignRepository, membershipRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new AddCampaignMemberCommand(Company, Guid.NewGuid(), "Lead", Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AddCampaignMember_NewMember_CreatesAndPersistsMembership()
    {
        var campaignId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var campaignRepository = Substitute.For<ICampaignRepository>();
        campaignRepository.ExistsAsync(Company, campaignId, Arg.Any<CancellationToken>()).Returns(true);
        var membershipRepository = Substitute.For<ICampaignMembershipRepository>();
        membershipRepository.FindAsync(Company, campaignId, "Lead", memberId, Arg.Any<CancellationToken>()).Returns((CampaignMembership?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AddCampaignMemberCommandHandler(campaignRepository, membershipRepository, unitOfWork);

        var result = await handler.Handle(new AddCampaignMemberCommand(Company, campaignId, "Lead", memberId), CancellationToken.None);

        result.CampaignId.Should().Be(campaignId);
        result.EntityType.Should().Be("Lead");
        result.EntityId.Should().Be(memberId);
        await membershipRepository.Received(1).AddAsync(Arg.Any<CampaignMembership>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddCampaignMember_AlreadyEnrolled_ReturnsExistingMembershipWithoutDuplicateInsertOrThrow()
    {
        var campaignId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var existing = CampaignMembership.Create(Company, campaignId, "Contact", memberId);
        var campaignRepository = Substitute.For<ICampaignRepository>();
        campaignRepository.ExistsAsync(Company, campaignId, Arg.Any<CancellationToken>()).Returns(true);
        var membershipRepository = Substitute.For<ICampaignMembershipRepository>();
        membershipRepository.FindAsync(Company, campaignId, "Contact", memberId, Arg.Any<CancellationToken>()).Returns(existing);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AddCampaignMemberCommandHandler(campaignRepository, membershipRepository, unitOfWork);

        var result = await handler.Handle(new AddCampaignMemberCommand(Company, campaignId, "Contact", memberId), CancellationToken.None);

        result.Id.Should().Be(existing.Id);
        await membershipRepository.DidNotReceive().AddAsync(Arg.Any<CampaignMembership>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveCampaignMember_WhenEnrolled_RemovesAndSaves()
    {
        var campaignId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var existing = CampaignMembership.Create(Company, campaignId, "Lead", memberId);
        var membershipRepository = Substitute.For<ICampaignMembershipRepository>();
        membershipRepository.FindAsync(Company, campaignId, "Lead", memberId, Arg.Any<CancellationToken>()).Returns(existing);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RemoveCampaignMemberCommandHandler(membershipRepository, unitOfWork);

        await handler.Handle(new RemoveCampaignMemberCommand(Company, campaignId, "Lead", memberId), CancellationToken.None);

        membershipRepository.Received(1).Remove(existing);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveCampaignMember_WhenNotEnrolled_ThrowsKeyNotFound()
    {
        var membershipRepository = Substitute.For<ICampaignMembershipRepository>();
        membershipRepository.FindAsync(Company, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CampaignMembership?)null);
        var handler = new RemoveCampaignMemberCommandHandler(membershipRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RemoveCampaignMemberCommand(Company, Guid.NewGuid(), "Lead", Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
