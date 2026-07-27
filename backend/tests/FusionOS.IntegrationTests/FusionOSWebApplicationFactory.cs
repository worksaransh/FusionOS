using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Companies;
using FusionOS.Modules.Core.Domain.Identity;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace FusionOS.IntegrationTests;

/// <summary>
/// Boots the real Host (FusionOS.Api.Host's Program) against a disposable
/// Postgres container (Testcontainers) instead of mocks — the integration test
/// scaffolding the enterprise audit flagged as entirely missing. Requires
/// Docker to be available wherever these tests run (the same requirement as
/// docker-compose.yml for local dev); there is no in-sandbox way to verify
/// this here since this environment has neither Docker nor the .NET SDK — see
/// README known-gaps. Runs the Host in the "Testing" environment so
/// Program.cs's fail-fast secret check (07_SECURITY.md) does not fire.
///
/// <b>Schema creation (found while adding RBAC/tenant-isolation coverage):</b>
/// this repo has never scaffolded a single EF Core migration for any module —
/// there is no Migrations/ folder anywhere under src/Modules, and neither
/// Program.cs nor any module's RegisterServices calls Database.Migrate(). A
/// fresh Testcontainers Postgres is therefore schema-less; every one of these
/// tests (including the two pre-existing ones, once they touch a real table)
/// would fail with an "relation does not exist" Postgres error without
/// something creating the tables first. <see cref="InitializeAsync"/> below
/// calls Database.EnsureCreatedAsync() per module DbContext instead of Migrate()
/// — EnsureCreated builds the schema straight from the current model, which is
/// the only workable option given no migrations exist, and is an accepted
/// pattern for a disposable per-test-class database that lives only as long as
/// this fixture does. Only Core and Crm are created — the two schemas this
/// test project's fixtures actually touch; every other module's schema is left
/// absent, same scope as the original two tests (Auth + Health).
///
/// <b>Seeding helpers:</b> the anonymous "bootstrap the first user of a brand
/// new company" path (POST /api/v1/core/auth/register with no bearer token)
/// cannot currently be exercised through the real HTTP pipeline — see the
/// remark on <see cref="SeedCompanyWithOwnerAsync"/> for why — so
/// <see cref="SeedCompanyWithOwnerAsync"/> and <see cref="SeedUserWithPermissionsAsync"/>
/// seed a company/user/role directly through the same repository interfaces
/// (IUserRepository/ICompanyRepository) the real command handlers use, then
/// hand back plain credentials so tests still authenticate for real via
/// POST /api/v1/core/auth/login and exercise every endpoint under test over
/// genuine HTTP.
/// </summary>
public sealed class FusionOSWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlBuilder _postgresBuilder = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("fusionos_test")
        .WithUsername("fusionos")
        .WithPassword("fusionos");

    private PostgreSqlContainer? _postgres;

    public async Task InitializeAsync()
    {
        _postgres = _postgresBuilder.Build();
        await _postgres.StartAsync();

        // Accessing Services builds the WebApplicationFactory's host now (using the
        // connection string just set above), rather than lazily on the first
        // CreateClient() call — needed so the schema exists before any test runs.
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CoreDbContext>().Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<CrmDbContext>().Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres?.GetConnectionString(),
            });
        });
    }

    /// <summary>
    /// Seeds a brand-new Company plus its bootstrap "Owner" user (every permission
    /// in PermissionCatalog granted), and returns their ids plus the plaintext
    /// password so the caller can log in for real over HTTP.
    ///
    /// <b>Why this doesn't just call POST /api/v1/core/auth/register:</b> that
    /// endpoint is [AllowAnonymous] specifically so a brand-new company's first
    /// user can bootstrap without already holding a token (AuthController's own
    /// doc comment). But RegisterUserCommand has a non-nullable Guid "CompanyId"
    /// property, and TenantIsolationBehavior (BuildingBlocks.Application) checks
    /// *every* request with such a property — reflectively, regardless of
    /// [AllowAnonymous] or IRequirePermission — against ICurrentUserContext.CompanyId,
    /// throwing ForbiddenException whenever they don't match. An anonymous caller
    /// has no CompanyId claim at all (null), so it never matches the freshly
    /// created company's id, and the bootstrap call 403s before
    /// RegisterUserCommandHandler's own (correct) "brand new company, no users
    /// yet" bootstrap logic ever runs. CreateCompanyCommand avoids this by
    /// deliberately having no CompanyId property at all (its own doc comment
    /// explains this is "the bootstrap action of a fresh tenant"); RegisterUserCommand
    /// was never given the same exemption when TenantIsolationBehavior was added.
    /// This looks like a genuine gap between the two behaviors, not intended
    /// design — flagged here rather than silently worked around, since it means
    /// today, over real HTTP, a freshly deployed FusionOS with zero companies/users
    /// has no way to create its very first login. Seeding through the repository
    /// interfaces directly (same calls RegisterUserCommandHandler itself would
    /// make) sidesteps the broken endpoint without weakening what these tests
    /// actually verify — every assertion below still goes over a real HTTP
    /// login/request, never a hand-rolled token.
    /// </summary>
    public async Task<(Guid CompanyId, Guid OwnerId, string Email, string Password)> SeedCompanyWithOwnerAsync(string companyName)
    {
        const string password = "P@ssw0rd-Test-1!";
        var email = $"owner-{Guid.NewGuid():N}@fusionos-test.local";

        using var scope = Services.CreateScope();
        var companies = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var company = Company.Create(companyName, $"{companyName} Legal Entity", "USD");
        await companies.AddAsync(company);
        await unitOfWork.SaveChangesAsync();

        var user = User.Register(email, "Test Owner", hasher.Hash(password));
        await users.AddAsync(user, CancellationToken.None);
        var ownerRoleId = await users.GetOrCreateCompanyOwnerRoleAsync(company.Id, CancellationToken.None);
        await users.LinkUserToCompanyAsync(user.Id, company.Id, ownerRoleId, branchId: null, CancellationToken.None);
        await unitOfWork.SaveChangesAsync();

        return (company.Id, user.Id, email, password);
    }

    /// <summary>
    /// Seeds a second user into an already-existing company, holding a brand-new
    /// company-scoped role granted exactly <paramref name="permissionCodes"/> (pass
    /// an empty list for a user with zero permissions — narrower than even the
    /// zero-permission "Member" role RegisterUserCommandHandler lands teammates on,
    /// which is exactly what a permission-denial test needs). Uses the same
    /// IUserRepository.AddRoleAsync/SetRolePermissionsAsync calls
    /// RolesController's real Create+SetPermissions actions use.
    /// </summary>
    public async Task<(Guid UserId, string Email, string Password)> SeedUserWithPermissionsAsync(Guid companyId, IReadOnlyList<string> permissionCodes)
    {
        const string password = "P@ssw0rd-Test-1!";
        var email = $"member-{Guid.NewGuid():N}@fusionos-test.local";

        using var scope = Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var user = User.Register(email, "Test Member", hasher.Hash(password));
        await users.AddAsync(user, CancellationToken.None);
        await unitOfWork.SaveChangesAsync();

        var role = Role.CreateCompanyRole(companyId, $"Test-Role-{Guid.NewGuid():N}");
        await users.AddRoleAsync(role, CancellationToken.None);
        await unitOfWork.SaveChangesAsync();

        await users.SetRolePermissionsAsync(role.Id, permissionCodes, CancellationToken.None);
        await users.LinkUserToCompanyAsync(user.Id, companyId, role.Id, branchId: null, CancellationToken.None);
        await unitOfWork.SaveChangesAsync();

        return (user.Id, email, password);
    }
}
