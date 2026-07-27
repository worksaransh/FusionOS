using System.Text.Json;
using System.Text.Json.Nodes;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using MediatR;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ValidateMigrationStagingRecords;

/// <summary>
/// Per-record-type rules, deliberately simple and honest rather than exhaustive:
/// - LEDGER / STOCKITEM / GROUP: must have a non-empty "Name" (the field the extractor
///   fills from Tally's NAME attribute — every Tally master carries one).
/// - VOUCHER: must have a non-empty "DATE" and at least one "ALLLEDGERENTRIES.LIST"
///   entry (the debit/credit lines) — a voucher with no ledger entries isn't a real
///   transaction.
/// - Any other/unrecognized record type: no rule is applied yet and it is marked
///   Mapped as-is — documented here rather than silently invented, since inventing a
///   plausible-looking rule for a type nobody has asked to migrate would be worse than
///   admitting there isn't one yet.
/// </summary>
public sealed class ValidateMigrationStagingRecordsCommandHandler : IRequestHandler<ValidateMigrationStagingRecordsCommand, MigrationJobDto>
{
    private readonly IMigrationJobRepository _jobRepository;
    private readonly IMigrationStagingRecordRepository _stagingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ValidateMigrationStagingRecordsCommandHandler(
        IMigrationJobRepository jobRepository,
        IMigrationStagingRecordRepository stagingRepository,
        IUnitOfWork unitOfWork)
    {
        _jobRepository = jobRepository;
        _stagingRepository = stagingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MigrationJobDto> Handle(ValidateMigrationStagingRecordsCommand request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.CompanyId, request.MigrationJobId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.MigrationJobId), $"Migration job '{request.MigrationJobId}' does not exist for this company."),
            });

        var pendingRecords = await _stagingRepository.ListByJobIdAndStatusAsync(request.CompanyId, request.MigrationJobId, MappingStatus.Pending, cancellationToken);

        job.StartValidation();

        foreach (var record in pendingRecords)
        {
            var errors = ValidateRecord(record.RecordType, record.RawDataJson);
            if (errors.Count == 0)
                record.MarkMapped();
            else
                record.MarkValidationFailed(JsonSerializer.Serialize(errors));
        }

        job.CompleteValidation();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MigrationJobMapper.ToDto(job);
    }

    private static List<string> ValidateRecord(string recordType, string rawDataJson)
    {
        var errors = new List<string>();

        JsonObject? obj;
        try
        {
            obj = JsonNode.Parse(rawDataJson) as JsonObject;
        }
        catch (JsonException)
        {
            errors.Add("Raw staged data is not valid JSON.");
            return errors;
        }

        if (obj is null)
        {
            errors.Add("Raw staged data is not a JSON object.");
            return errors;
        }

        switch (recordType)
        {
            case "LEDGER":
            case "STOCKITEM":
            case "GROUP":
                if (string.IsNullOrWhiteSpace(GetString(obj, "Name")))
                    errors.Add($"{recordType} record is missing a non-empty Name.");
                break;

            case "VOUCHER":
                if (string.IsNullOrWhiteSpace(GetString(obj, "DATE")))
                    errors.Add("Voucher record is missing a non-empty DATE.");

                var entries = obj["ALLLEDGERENTRIES.LIST"];
                var hasLedgerEntries = entries switch
                {
                    JsonArray array => array.Count > 0,
                    JsonObject => true,
                    _ => false,
                };
                if (!hasLedgerEntries)
                    errors.Add("Voucher record has no ledger entry lines (ALLLEDGERENTRIES.LIST).");
                break;

            default:
                // No rule defined yet for this record type — see class doc comment.
                break;
        }

        return errors;
    }

    /// <summary>Case-insensitive lookup — the extractor preserves Tally's own element-name casing verbatim, so callers shouldn't need to guess it exactly.</summary>
    private static string? GetString(JsonObject obj, string key)
    {
        foreach (var property in obj)
        {
            if (string.Equals(property.Key, key, StringComparison.OrdinalIgnoreCase))
                return property.Value?.GetValue<string>();
        }
        return null;
    }
}
