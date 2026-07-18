using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Users.Commands.AssignUserRole;

public sealed class AssignUserRoleCommandHandler : IRequestHandler<AssignUserRoleCommand, Unit>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public AssignUserRoleCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
    {
        _ = await _users.GetRoleByIdAsync(request.RoleId, request.CompanyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{request.RoleId}' was not found.");

        var alreadyInCompany = await _users.GetCompanyRolesAsync(request.UserId, cancellationToken);
        if (!alreadyInCompany.Any(x => x.CompanyId == request.CompanyId))
            throw new KeyNotFoundException($"User '{request.UserId}' is not a member of this company.");

        await _users.AssignUserRoleAsync(request.UserId, request.CompanyId, request.RoleId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
