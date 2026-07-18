namespace FusionOS.Modules.Core.Domain.Workflow;

/// <summary>
/// Shared by both ApprovalRequest.Status (the request as a whole) and
/// ApprovalStep.Decision (one step within it) — stored as text via EF value
/// conversion, never a native PostgreSQL enum (04_DATABASE_GUIDELINES.md
/// §10, same convention as Procurement's PurchaseOrderStatus).
/// </summary>
public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
}
