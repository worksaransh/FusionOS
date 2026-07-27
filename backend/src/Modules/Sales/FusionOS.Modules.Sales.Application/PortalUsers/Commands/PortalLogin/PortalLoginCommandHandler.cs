using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;
using FluentValidation.Results;
using MediatR;

namespace FusionOS.Modules.Sales.Application.PortalUsers.Commands.PortalLogin;

/// <summary>
/// Issues a customer portal access token. This is the one place the
/// architectural boundary between Sales and Core's Auth stack matters most,
/// so the reasoning lives here rather than being scattered:
///
/// Core.Application.Auth.Contracts.IJwtTokenService/IPasswordHasher (and their
/// BCrypt/JWT implementations in Core.Infrastructure.Security) are NOT
/// published anywhere Sales could legally reference - they live in
/// Core.Application/Core.Infrastructure proper, not in
/// FusionOS.BuildingBlocks.Application or FusionOS.SharedKernel (the only two
/// projects every module, including Sales, is allowed to reference - see
/// Sales.Application.csproj/Sales.Infrastructure.csproj's own ProjectReferences
/// and 03_SYSTEM_ARCHITECTURE.md §2's module-boundary rule that
/// ModuleBoundaryTests.cs enforces for Core.Domain). Taking a new
/// Sales -> Core.Application/Core.Infrastructure ProjectReference just for this
/// would be exactly the illegal cross-module dependency that rule exists to
/// prevent - a customer is not a company employee, and Sales must not need to
/// know anything about Core's staff identity model to authenticate one.
///
/// Instead, IPortalPasswordHasher/IPortalTokenService are small,
/// self-contained mirrors of Core's own contracts, implemented in
/// Sales.Infrastructure.Security using the SAME NuGet packages
/// (BCrypt.Net-Next, System.IdentityModel.Tokens.Jwt) Core.Infrastructure
/// already depends on - these are already solution-wide dependencies, so
/// referencing them from Sales.Infrastructure's own .csproj installs nothing
/// new. The JWT is signed with the SAME "Jwt:SigningKey"/"Jwt:Issuer"/
/// "Jwt:Audience" configuration Core's JwtTokenService uses, so the single
/// JWT bearer authentication scheme registered once in Program.cs can
/// validate both staff and customer tokens without a second scheme - the two
/// kinds of token are told apart purely by claims ("token_type": "customer"
/// plus "customer_id", and the deliberate ABSENCE of any "permission" claim).
///
/// That absence is what keeps a customer token from ever passing a staff
/// endpoint's authorization: AuthorizationBehavior (BuildingBlocks.Application)
/// only enforces IRequirePermission.RequiredPermissions against
/// ICurrentUserContext.Permissions, which HttpCurrentUserContext populates
/// exclusively from "permission" claims on the validated JWT. A customer
/// token carries none, so HasPermission(...) is false for every staff
/// permission code and every staff command/query that implements
/// IRequirePermission (confirmed: ListQuotationsQuery/ListSalesOrdersQuery/
/// ListInvoicesQuery/ListDispatchesQuery/AcceptQuotationCommand/
/// RejectQuotationCommand all do) throws ForbiddenException for a customer
/// caller. Portal-only endpoints/queries in this file's sibling folders
/// therefore never implement IRequirePermission at all - their authorization
/// is the PortalOnly claim policy (Program.cs) plus deriving CustomerId from
/// the token rather than trusting a request parameter.
///
/// No refresh token is issued in this minimal slice (unlike Core's own
/// login) - a short-lived access token that the customer re-requests via
/// PortalLoginPage is a deliberately smaller surface for a first cut; adding
/// portal refresh tokens is a documented, explicit follow-up, not attempted here.
/// </summary>
public sealed class PortalLoginCommandHandler : IRequestHandler<PortalLoginCommand, PortalLoginResult>
{
    private readonly ICustomerPortalUserRepository _repository;
    private readonly IPortalPasswordHasher _hasher;
    private readonly IPortalTokenService _tokenService;

    public PortalLoginCommandHandler(ICustomerPortalUserRepository repository, IPortalPasswordHasher hasher, IPortalTokenService tokenService)
    {
        _repository = repository;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<PortalLoginResult> Handle(PortalLoginCommand request, CancellationToken cancellationToken)
    {
        var candidates = await _repository.GetByEmailAsync(request.Email, cancellationToken);

        // Email is only unique per-Customer (see CustomerPortalUser's doc
        // comment), so more than one row can share this email across
        // unrelated customers - find the one candidate (if any) that is both
        // active AND whose hash matches this password. Deliberately the SAME
        // error for "no such user", "wrong password", and "inactive account"
        // (same anti-enumeration convention as Core's own LoginCommandHandler)
        // - a distinct message per case would let a caller probe which
        // accounts exist or are merely disabled.
        var match = candidates.FirstOrDefault(c => c.IsActive && _hasher.Verify(request.Password, c.PasswordHash));

        if (match is null)
            throw new ValidationException(new[] { new ValidationFailure("email", "Invalid email or password.") });

        var token = _tokenService.GenerateAccessToken(new PortalTokenClaims(match.Id, match.CustomerId, match.Email));

        return new PortalLoginResult(match.Id, match.CustomerId, match.Email, token.Value, token.ExpiresAt);
    }
}
