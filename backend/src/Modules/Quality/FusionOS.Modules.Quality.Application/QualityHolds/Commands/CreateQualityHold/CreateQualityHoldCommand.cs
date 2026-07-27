using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Commands.CreateQualityHold;

/// <summary>
/// Places a hold on some (EntityType, EntityId) pair the caller owns — same convention as
/// Core's CreateApprovalRequestCommand: EntityType/EntityId double as the IAuditableCommand
/// identity too, since the audit trail for "a hold was placed" is naturally about the same
/// target entity the hold itself is for.
/// </summary>
public sealed record CreateQualityHoldCommand(Guid CompanyId, string EntityType, Guid EntityId, string Reason, Guid? InspectionId)
    : ICommand<QualityHoldDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.quality-hold.create" };
    public string Action => "Created";
}
