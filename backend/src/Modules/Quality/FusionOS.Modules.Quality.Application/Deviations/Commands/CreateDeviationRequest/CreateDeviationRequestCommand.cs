using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Deviations.Contracts;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.CreateDeviationRequest;

public sealed record CreateDeviationRequestCommand(Guid CompanyId, Guid? InspectionId, string Reason, Guid RequestedByUserId)
    : ICommand<DeviationRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.deviation.create" };
    public string EntityType => nameof(Domain.Deviations.DeviationRequest);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
