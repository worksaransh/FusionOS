using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Sales.Application.PortalUsers.Commands.CreateCustomerPortalUser;

public sealed record CreateCustomerPortalUserResult(Guid Id, Guid CustomerId, string Email);

/// <summary>
/// Staff-side provisioning: a customer cannot self-register for the portal in
/// this minimal slice (deliberately out of scope - see the overall task
/// report), so a staff user with permission to manage the Customer creates
/// the portal login on the customer's behalf. Reuses the existing
/// "sales.customer.update" permission rather than adding a new code to
/// PermissionCatalog.cs (out of bounds for this task) - provisioning a
/// customer's own portal credential is a reasonable extension of "update this
/// customer", not a distinct capability that needs its own grant.
/// </summary>
public sealed record CreateCustomerPortalUserCommand(Guid CompanyId, Guid CustomerId, string Email, string Password)
    : ICommand<CreateCustomerPortalUserResult>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.customer.update" };
}
