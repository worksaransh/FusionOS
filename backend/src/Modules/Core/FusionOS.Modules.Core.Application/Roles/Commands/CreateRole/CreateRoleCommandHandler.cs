using FluentValidation.Results;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Roles.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using MediatR;

namespace FusionOS.Modules.Core.Application.Roles.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _users.RoleNameExistsAsync(request.CompanyId, request.Name, null, cancellationToken))
            throw new ValidationException(new[] { new ValidationFailure("name", "A role with that name already exists.") });

        var role = Role.CreateCompanyRole(request.CompanyId, request.Name.Trim());
        await _users.AddRoleAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleDto(role.Id, role.Name, role.IsSystemRole);
    }
}
