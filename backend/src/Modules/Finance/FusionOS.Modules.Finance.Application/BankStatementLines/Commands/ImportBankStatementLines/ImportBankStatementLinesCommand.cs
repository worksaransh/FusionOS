using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ImportBankStatementLines;

/// <summary>
/// Bulk-imports bank statement lines already parsed by
/// BankStatementLinesController from an uploaded CSV file (see the
/// controller's own doc comment for the supported format — CSV parsing
/// itself is deliberately an Api-layer concern, not Application's, mirroring
/// how DocumentsController reads the raw IFormFile before ever calling into
/// MediatR). Lines is the set of rows the controller could parse; rows it
/// couldn't parse never reach this command at all — the controller merges
/// its own parse failures into the final response after this command
/// returns (see BankStatementLineImportResultDto's doc comment).
///
/// Unlike RecordStatementLineCommand, a single bad or duplicate row here
/// does not fail the whole request — see
/// ImportBankStatementLinesCommandHandler for the per-row handling.
/// </summary>
public sealed record ImportBankStatementLinesCommand(Guid CompanyId, Guid BankAccountId, IReadOnlyList<ImportBankStatementLineInput> Lines)
    : ICommand<BankStatementLineImportResultDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.bank-statement-line.import" };
    public string EntityType => nameof(Domain.BankStatementLines.BankStatementLine);
    public Guid EntityId { get; init; }
    public string Action => "Bulk-imported";
}
