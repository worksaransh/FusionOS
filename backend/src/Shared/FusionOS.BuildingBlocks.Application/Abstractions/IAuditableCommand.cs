namespace FusionOS.BuildingBlocks.Application.Abstractions;

/// <summary>Marks a command whose successful execution must produce an audit log entry.</summary>
public interface IAuditableCommand
{
    string EntityType { get; }
    Guid EntityId { get; }
    string Action { get; }
}
