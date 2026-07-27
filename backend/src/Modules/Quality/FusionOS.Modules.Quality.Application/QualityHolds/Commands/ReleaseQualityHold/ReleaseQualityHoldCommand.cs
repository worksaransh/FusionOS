using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;

namespace FusionOS.Modules.Quality.Application.QualityHolds.Commands.ReleaseQualityHold;

public sealed record ReleaseQualityHoldCommand(Guid CompanyId, Guid QualityHoldId, string? Note)
    : ICommand<QualityHoldDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.quality-hold.release" };
    public string EntityType => nameof(Domain.QualityHolds.QualityHold);
    public Guid EntityId => QualityHoldId;
    public string Action => "Released";
}
