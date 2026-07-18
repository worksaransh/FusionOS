using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;

public sealed record CreateInspectionCommand(Guid CompanyId, InspectionType Type, Guid ReferenceId, IReadOnlyList<string> Characteristics)
    : ICommand<InspectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.inspection.create" };
    public string EntityType => nameof(Domain.Inspections.Inspection);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
