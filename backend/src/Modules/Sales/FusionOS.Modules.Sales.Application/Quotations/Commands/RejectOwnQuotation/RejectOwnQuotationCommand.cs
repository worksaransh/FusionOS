using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.RejectOwnQuotation;

/// <summary>
/// Portal counterpart of RejectQuotationCommand - see AcceptOwnQuotationCommand's doc comment for the full reasoning.
/// Implements IPubliclyAccessible (Phase 9 hardening fix, 2026-07-25): gated by the customer-portal
/// JWT/policy rather than staff RBAC — exempt from AuthorizationBehavior's fail-closed check.
/// </summary>
public sealed record RejectOwnQuotationCommand(Guid CustomerId, Guid QuotationId) : ICommand<QuotationDto>, IPubliclyAccessible;
