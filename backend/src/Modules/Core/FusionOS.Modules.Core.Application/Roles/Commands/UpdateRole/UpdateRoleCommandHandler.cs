using FluentValidation.Results;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Roles.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Roles.Commands.UpdateRole;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _users.GetRoleByIdAsync(request.RoleId, request.CompanyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Role '{request.RoleId}' was not found.");

        if (await _users.RoleNameExistsAsync(request.CompanyId, request.Name, request.RoleId, cancellationToken))
            throw new ValidationException(new[] { new ValidationFailure("name", "A role with that name already exists.") });

        try
        {
            role.Rename(request.Name.Trim());
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationException(new[] { new ValidationFailure("roleId", ex.Message) });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RoleDto(role.Id, role.Name, role.IsSystemRole);
    }
}
