using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Behaviors;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Billing.Application.Entitlements;
using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Application.Subscriptions.Contracts;
using FusionOS.Modules.Billing.Domain.Plans;
using FusionOS.Modules.Billing.Domain.Subscriptions;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Billing.Tests;

/// <summary>
/// Covers the two collaborators that together enforce "does this company's plan include this
/// module": PlanEntitlementService (the fail-open-by-default lookup against Subscription/Plan)
/// and PlanEntitlementBehavior (the MediatR pipeline behavior that calls it and turns a "no"
/// into a ForbiddenException). Deliberately unit-level — no database, no HTTP — because the
/// fail-open contract this proves ("no Subscription row" / "Billing data unreachable" => always
/// allowed) is exactly the thing that must never regress, and a real DB round trip would only
/// obscure that with unrelated setup.
/// </summary>
public sealed class PlanEntitlementBehaviorTests
{
    private sealed record FakeRequest(string[] RequiredPermissions) : IRequirePermission, IRequest<string>;

    private readonly ISubscriptionRepository _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
    private readonly IPlanRepository _planRepository = Substitute.For<IPlanRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly IPlanEntitlementService _service;
    private readonly PlanEntitlementBehavior<FakeRequest, string> _behavior;

    private readonly Guid _companyId = Guid.NewGuid();

    public PlanEntitlementBehaviorTests()
    {
        _service = new PlanEntitlementService(_subscriptionRepository, _planRepository, Substitute.For<ILogger<PlanEntitlementService>>());
        _currentUser.CompanyId.Returns(_companyId);
        _behavior = new PlanEntitlementBehavior<FakeRequest, string>(_currentUser, _service);
    }

    private static Task<string> Next() => Task.FromResult("ok");

    // (a) No Subscription row at all -> allowed (fail-open backward-compatibility default for
    // companies that predate the Billing module, and for tests that seed a Company directly).
    [Fact]
    public async Task NoSubscriptionForCompany_AllowsThrough()
    {
        _subscriptionRepository.GetByCompanyIdAsync(_companyId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var request = new FakeRequest(new[] { "manufacturing.work-order.create" });

        var result = await _behavior.Handle(request, () => Next(), CancellationToken.None);

        result.Should().Be("ok");
    }

    // (b) Active Subscription whose Plan includes the module -> allowed.
    [Fact]
    public async Task ActiveSubscription_PlanIncludesModule_AllowsThrough()
    {
        var plan = Plan.Create("GROWTH", "Growth", PlanTier.Growth, 25m, includedModules: "core,manufacturing,inventory");
        var subscription = Subscription.Create(_companyId, plan.Id, trialEndsAt: null);
        subscription.Activate();

        _subscriptionRepository.GetByCompanyIdAsync(_companyId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planRepository.GetByIdAsync(plan.Id, Arg.Any<CancellationToken>()).Returns(plan);

        var request = new FakeRequest(new[] { "manufacturing.work-order.create" });

        var result = await _behavior.Handle(request, () => Next(), CancellationToken.None);

        result.Should().Be("ok");
    }

    // (c) Active Subscription whose Plan does NOT include the module -> blocked with a
    // ForbiddenException carrying an upgrade-oriented message (same exception type
    // AuthorizationBehavior throws for a denied permission, per the task's "consistent,
    // already-tested error shape" requirement).
    [Fact]
    public async Task ActiveSubscription_PlanExcludesModule_Blocks()
    {
        var plan = Plan.Create("STARTER", "Starter", PlanTier.Starter, 10m, includedModules: "core,inventory");
        var subscription = Subscription.Create(_companyId, plan.Id, trialEndsAt: null);
        subscription.Activate();

        _subscriptionRepository.GetByCompanyIdAsync(_companyId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planRepository.GetByIdAsync(plan.Id, Arg.Any<CancellationToken>()).Returns(plan);

        var request = new FakeRequest(new[] { "manufacturing.work-order.create" });

        var act = () => _behavior.Handle(request, () => Next(), CancellationToken.None);

        var thrown = await act.Should().ThrowAsync<ForbiddenException>();
        thrown.Which.Message.Should().Contain("manufacturing").And.Contain("Upgrade");
    }

    // (d) A request requiring only "core.*" permissions is always allowed, regardless of plan —
    // proven here against a Plan that excludes "core" outright, which should never matter.
    [Fact]
    public async Task CoreOnlyPermissions_AlwaysAllowed_RegardlessOfPlan()
    {
        var plan = Plan.Create("BARE", "Bare", PlanTier.Starter, 0m, includedModules: string.Empty);
        var subscription = Subscription.Create(_companyId, plan.Id, trialEndsAt: null);
        subscription.Activate();

        _subscriptionRepository.GetByCompanyIdAsync(_companyId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planRepository.GetByIdAsync(plan.Id, Arg.Any<CancellationToken>()).Returns(plan);

        var request = new FakeRequest(new[] { "core.user.register" });

        var result = await _behavior.Handle(request, () => Next(), CancellationToken.None);

        result.Should().Be("ok");
        // Confirms the "core" fast path never even asks the entitlement service.
        await _planRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // Directly covers IPlanEntitlementService's other documented fail-open path: Billing's own
    // data being unreachable for any reason (e.g. schema not provisioned in a given database)
    // must never itself deny access.
    [Fact]
    public async Task Service_BillingDataUnreachable_FailsOpen()
    {
        _subscriptionRepository.GetByCompanyIdAsync(_companyId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Subscription?>(new InvalidOperationException("relation \"billing.subscriptions\" does not exist")));

        var included = await _service.IsModuleIncludedAsync(_companyId, "manufacturing", CancellationToken.None);

        included.Should().BeTrue();
    }
}
