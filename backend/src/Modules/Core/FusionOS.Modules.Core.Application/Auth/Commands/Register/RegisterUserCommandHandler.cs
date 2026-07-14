using FluentValidation.Results;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Register;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;

    public RegisterUserCommandHandler(IUserRepository users, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Bootstrap case: a brand-new company with no users yet - anyone holding the
        // company's id may register its first (Owner) user. Every registration after
        // that is an "invite a teammate" action and requires core.user.register,
        // since IRequirePermission alone cannot express a data-dependent rule like
        // "only for companies that already have a user" (07_SECURITY.md §2).
        var companyAlreadyHasUsers = await _users.CompanyHasAnyUsersAsync(request.CompanyId, cancellationToken);
        if (companyAlreadyHasUsers && !_currentUser.HasPermission("core.user.register"))
            throw new ForbiddenException("core.user.register");

        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            throw new ValidationException(new[] { new ValidationFailure("email", "That email is already registered.") });

        var user = User.Register(request.Email, request.FullName, _passwordHasher.Hash(request.Password));
        await _users.AddAsync(user, cancellationToken);

        var ownerRoleId = await _users.GetOrCreateCompanyOwnerRoleAsync(request.CompanyId, cancellationToken);
        await _users.LinkUserToCompanyAsync(user.Id, request.CompanyId, ownerRoleId, branchId: null, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.Email, user.FullName, user.IsActive, user.CreatedAt);
    }
}
