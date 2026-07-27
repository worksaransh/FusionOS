using System.Text.Json.Nodes;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Commands.CreateAccount;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.LoadMigrationStagingRecords;

/// <summary>
/// Loads every Mapped staging record for a job into a real FusionOS record via a real
/// Application-layer command — never direct SQL (11_SAP_MIGRATION.md §2). Only the Tally
/// LEDGER → Finance Account path is implemented for real this pass:
/// <list type="bullet">
/// <item>LEDGER records call <see cref="CreateAccountCommand"/> with the ledger's staged
/// "Name" as both the account Name and the seed for its Code (Tally ledgers don't carry a
/// FusionOS-style account code, so one is derived), AccountType defaulted to "Asset" (a
/// real migration would map Tally's ledger PARENT group hierarchy to the correct
/// Asset/Liability/Equity/Revenue/Expense classification — that mapping table doesn't
/// exist yet, so this is a documented simplification, not a silent guess dressed up as
/// correct), and no ParentAccountId (chart-of-accounts hierarchy mapping is the same
/// kind of follow-up work).</item>
/// <item>VOUCHER → Finance Journal Entry and STOCKITEM → Inventory Product are the exact
/// same pattern (dispatch the record's real create-command via ISender, catch its
/// validation failure per-record) but are NOT implemented here — left as the obvious,
/// straightforwardly repeatable next slice.</item>
/// </list>
/// A single bad staging record's create-command failure is caught and recorded against
/// that record (MappingStatus stays Mapped but it is skipped from TotalRecordsLoaded and
/// counted as failed) rather than aborting the whole batch — one bad Tally ledger
/// shouldn't block migrating the other 999.
/// </summary>
public sealed class LoadMigrationStagingRecordsCommandHandler : IRequestHandler<LoadMigrationStagingRecordsCommand, MigrationJobDto>
{
    private readonly IMigrationJobRepository _jobRepository;
    private readonly IMigrationStagingRecordRepository _stagingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISender _sender;

    public LoadMigrationStagingRecordsCommandHandler(
        IMigrationJobRepository jobRepository,
        IMigrationStagingRecordRepository stagingRepository,
        IUnitOfWork unitOfWork,
        ISender sender)
    {
        _jobRepository = jobRepository;
        _stagingRepository = stagingRepository;
        _unitOfWork = unitOfWork;
        _sender = sender;
    }

    public async Task<MigrationJobDto> Handle(LoadMigrationStagingRecordsCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.CompanyId, request.MigrationJobId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.MigrationJobId), $"Migration job '{request.MigrationJobId}' does not exist for this company."),
            });

        var mappedRecords = await _stagingRepository.ListByJobIdAndStatusAsync(request.CompanyId, request.MigrationJobId, MappingStatus.Mapped, cancellationToken);

        job.StartLoad();

        var loadedCount = 0;
        var failedCount = 0;

        foreach (var record in mappedRecords)
        {
            if (record.RecordType != "LEDGER")
            {
                // No real create-command wired for this record type yet — see class doc comment.
                // Not a data problem with the record itself, so it isn't marked Failed, but it also
                // can't be counted as loaded.
                failedCount++;
                continue;
            }

            try
            {
                var obj = JsonNode.Parse(record.RawDataJson) as JsonObject;
                var name = obj?["Name"]?.GetValue<string>();

                if (string.IsNullOrWhiteSpace(name))
                {
                    failedCount++;
                    continue;
                }

                var code = DeriveAccountCode(name);

                var account = await _sender.Send(
                    new CreateAccountCommand(request.CompanyId, code, name, "Asset", ParentAccountId: null),
                    cancellationToken);

                record.MarkLoaded("Account", account.Id);
                loadedCount++;
            }
            catch (ValidationException)
            {
                // e.g. duplicate account code, or any other business-rule rejection from Finance's
                // own CreateAccountCommandHandler — recorded as a failed load, batch keeps going.
                failedCount++;
            }
        }

        job.CompleteLoad(loadedCount, failedCount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MigrationJobMapper.ToDto(job);
    }

    /// <summary>
    /// Tally ledgers have a Name but no FusionOS-style short account Code, so one is derived:
    /// strip everything but letters/digits, uppercase, truncate to Finance's 20-char Code limit.
    /// Collisions across ledgers with similar names are expected and handled — Finance's own
    /// CodeExistsAsync check will reject the second one, which this handler treats as a per-record
    /// load failure rather than inventing de-duplication logic that would mask the real ambiguity.
    /// </summary>
    private static string DeriveAccountCode(string name)
    {
        var chars = name.Where(char.IsLetterOrDigit).ToArray();
        var code = new string(chars).ToUpperInvariant();
        if (code.Length == 0)
            code = "LEDGER";
        return code.Length > 20 ? code[..20] : code;
    }
}
