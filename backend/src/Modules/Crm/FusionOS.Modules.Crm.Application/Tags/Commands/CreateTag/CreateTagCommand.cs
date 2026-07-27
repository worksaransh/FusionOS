using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tags.Contracts;

namespace FusionOS.Modules.Crm.Application.Tags.Commands.CreateTag;

public sealed record CreateTagCommand(Guid CompanyId, string Name, string? Color)
    : ICommand<TagDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.tag.manage" };
    public string EntityType => nameof(Domain.Tags.Tag);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
