namespace FusionOS.Modules.Sales.Application.PortalUsers.Contracts;

/// <summary>
/// Sales-owned mirror of Core.Application.Auth.Contracts.IPasswordHasher.
/// Deliberately NOT the same interface - Sales cannot reference
/// Core.Application (no shared-kernel home exists for it today; see
/// PortalLoginCommandHandler's doc comment for the full module-boundary
/// reasoning), so this is a small, self-contained copy of the same contract,
/// implemented in Sales.Infrastructure using the same already-solution-wide
/// BCrypt.Net-Next package Core.Infrastructure uses.
/// </summary>
public interface IPortalPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string hash);
}
