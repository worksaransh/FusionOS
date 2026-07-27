using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Deviations.Contracts;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.ApproveDeviationRequest;

public sealed record ApproveDeviationRequestCommand(Guid CompanyId, Guid DeviationRequestId, Guid DecidedByUserId, string? Note)
    : ICommand<DeviationRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.deviation.approve" };
    public string EntityType => nameof(Domain.Deviations.DeviationRequest);
    public Guid EntityId => DeviationRequestId;
    public string Action => "Approved";
}
