using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateUserCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{request.UserId}' was not found.");

        var companyRoles = await _users.GetCompanyRolesAsync(request.UserId, cancellationToken);
        if (!companyRoles.Any(x => x.CompanyId == request.CompanyId))
            throw new KeyNotFoundException($"User '{request.UserId}' is not a member of this company.");

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
