namespace FusionOS.Modules.Core.Application.Users.Contracts;

/// <summary>A user linked to a company, with the single role they currently hold there.</summary>
public sealed record CompanyUserDto(Guid UserId, string Email, string FullName, Guid RoleId, string RoleName);
