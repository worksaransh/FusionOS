using FluentValidation;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ImportBankStatementLines;

/// <summary>
/// Validates only the command's own shape (CompanyId/BankAccountId
/// present, at least one row submitted) — per-row content problems (a zero
/// amount, an empty description) are deliberately left to
/// ImportBankStatementLinesCommandHandler/BankStatementLine.Create to catch
/// and report per row, not here, since a FluentValidation failure on one
/// row would reject the whole request rather than skip just that row.
/// </summary>
public sealed class ImportBankStatementLinesValidator : AbstractValidator<ImportBankStatementLinesCommand>
{
    public ImportBankStatementLinesValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one statement line row is required.");
    }
}
