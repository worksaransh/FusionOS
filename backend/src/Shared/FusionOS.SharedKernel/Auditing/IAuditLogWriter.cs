namespace FusionOS.SharedKernel.Auditing;

/// <summary>
/// Published contract for writing audit entries. Implemented by the Core module's
/// Infrastructure layer and registered once at the Host composition root, so every
/// module can depend on this abstraction without taking a reference to Core.
/// </summary>
public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
