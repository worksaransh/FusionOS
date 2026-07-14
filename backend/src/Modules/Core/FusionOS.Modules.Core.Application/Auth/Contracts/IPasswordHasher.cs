namespace FusionOS.Modules.Core.Application.Auth.Contracts;

/// <summary>Isolates the hashing algorithm (BCrypt in Infrastructure) from Application/Domain — 07_SECURITY.md.</summary>
public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string hash);
}
