using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.AcceptOwnQuotation;

/// <summary>
/// Portal counterpart of AcceptQuotationCommand, for a customer accepting
/// their OWN quotation. Deliberately a separate, thin command rather than
/// reusing AcceptQuotationCommand: that command is gated by
/// IRequirePermission (RequiredPermissions: "sales.quotation.accept"), which
/// a customer token can never satisfy (portal tokens carry zero "permission"
/// claims by design - see PortalLoginCommandHandler's doc comment), and
/// mixing a permission-gated staff command with a claim/ownership-gated
/// portal path in one class would make the staff command's authorization
/// story harder to reason about for no benefit. This command instead performs
/// the SAME domain operation (Quotation.Accept()) via
/// IQuotationRepository.GetByIdForCustomerAsync, which returns null (->
/// KeyNotFoundException, same as the staff path's 404 for an unknown id) for
/// any quotation that does not belong to the calling CustomerId - that
/// ownership check IS this command's authorization, there is no
/// IRequirePermission here at all.
/// Implements IPubliclyAccessible (Phase 10 audit fix, 2026-07-27): gated by the
/// customer-portal JWT/policy rather than staff RBAC — exempt from AuthorizationBehavior's
/// fail-closed check.
/// </summary>
public sealed record AcceptOwnQuotationCommand(Guid CustomerId, Guid QuotationId) : ICommand<QuotationDto>, IPubliclyAccessible;
