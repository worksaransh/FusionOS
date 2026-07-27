using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;

/// <summary>
/// Marks the asset disposed (FixedAsset.Dispose — always succeeds and is
/// always saved, regardless of GL configuration) and computes the disposal
/// gain/loss:
///
/// <list type="bullet">
/// <item>AccumulatedDepreciation = fixedAsset.CalculateAccumulatedDepreciation(DisposedDate)
/// — the same straight-line figure GetDepreciationScheduleQueryHandler
/// computes, rounded to the ledger's numeric(19,4) scale the same way
/// PostMonthlyDepreciationCommandHandler rounds its own monthly figure.</item>
/// <item>NetBookValue = AcquisitionCost - AccumulatedDepreciation.</item>
/// <item>GainOrLoss = DisposalProceeds - NetBookValue (positive = gain, negative = loss).</item>
/// </list>
///
/// <b>GL posting</b> — a single balanced JournalEntry when every account it
/// needs is available:
/// <list type="bullet">
/// <item>Debit the asset's own AccumulatedDepreciationAccountId for AccumulatedDepreciation
/// (clearing it) — same field PostMonthlyDepreciationCommandHandler credits, requires it be set.</item>
/// <item>Debit FinanceSettings.DefaultDisposalProceedsAccountId for DisposalProceeds,
/// only when DisposalProceeds is greater than zero (a pure discard has no proceeds line).</item>
/// <item>Debit (loss) or credit (gain) FinanceSettings.DefaultGainLossOnDisposalAccountId
/// for the magnitude of GainOrLoss, only when it is nonzero (proceeds exactly
/// equal to net book value needs no gain/loss line).</item>
/// <item>Credit the asset's own AssetAccountId for the full AcquisitionCost
/// (removing the original cost) — AssetAccountId is required on every
/// FixedAsset, so this line is always available.</item>
/// </list>
///
/// The entry balances by construction (the four lines above net to zero same
/// as any disposal journal: AccumulatedDepreciation + DisposalProceeds +
/// max(-GainOrLoss, 0) == AcquisitionCost + max(GainOrLoss, 0)).
///
/// <b>Silently skips GL posting</b> (same restraint as InvoiceIssuedConsumer's
/// own doc comment for its own optional GL post, applied here to a command
/// handler rather than an event consumer because unlike PostMonthlyDepreciationCommandHandler
/// — which throws a ValidationException and refuses to post anything when its
/// one required account is missing — a fixed-asset disposal is a real-world,
/// one-way event the caller cannot undo or defer just because Finance hasn't
/// finished configuring its Chart-of-Accounts mapping. So the disposal itself
/// always succeeds; only the GL entry is conditionally omitted) whenever:
/// the asset has no AccumulatedDepreciationAccountId configured, or
/// DisposalProceeds is positive but FinanceSettings has no
/// DefaultDisposalProceedsAccountId, or GainOrLoss is nonzero but
/// FinanceSettings has no DefaultGainLossOnDisposalAccountId. In every case
/// DisposeFixedAssetResultDto.JournalEntry is null but AccumulatedDepreciation/
/// NetBookValue/GainOrLoss are still returned — the caller always sees the
/// numbers even when nothing was posted.
/// </summary>
public sealed class DisposeFixedAssetCommandHandler : IRequestHandler<DisposeFixedAssetCommand, DisposeFixedAssetResultDto>
{
    private readonly IFixedAssetRepository _repository;
    private readonly IFinanceSettingsRepository _settingsRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DisposeFixedAssetCommandHandler(
        IFixedAssetRepository repository,
        IFinanceSettingsRepository settingsRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _settingsRepository = settingsRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DisposeFixedAssetResultDto> Handle(DisposeFixedAssetCommand request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _repository.GetByIdAsync(request.CompanyId, request.FixedAssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Fixed asset '{request.FixedAssetId}' was not found.");

        // Computed from the asset's own fields before Dispose() flips its state —
        // AcquisitionCost/SalvageValue/UsefulLifeMonths/AcquisitionDate are all
        // unaffected by disposal, so ordering here doesn't matter, but computing
        // first reads more naturally alongside the "always succeeds" doc comment.
        var accumulatedDepreciation = Math.Round(
            fixedAsset.CalculateAccumulatedDepreciation(request.DisposedDate), 4, MidpointRounding.AwayFromZero);
        var netBookValue = fixedAsset.AcquisitionCost - accumulatedDepreciation;
        var gainOrLoss = request.DisposalProceeds - netBookValue;

        fixedAsset.Dispose(request.DisposedDate);

        JournalEntryDto? journalEntryDto = null;

        var settings = await _settingsRepository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        var accumulatedDepreciationAccountId = fixedAsset.AccumulatedDepreciationAccountId;
        var needsProceedsAccount = request.DisposalProceeds > 0m;
        var needsGainLossAccount = gainOrLoss != 0m;
        var proceedsAccountId = settings?.DefaultDisposalProceedsAccountId;
        var gainLossAccountId = settings?.DefaultGainLossOnDisposalAccountId;

        var canPostToGl =
            accumulatedDepreciationAccountId.HasValue &&
            (!needsProceedsAccount || proceedsAccountId.HasValue) &&
            (!needsGainLossAccount || gainLossAccountId.HasValue);

        if (canPostToGl)
        {
            var reference = $"Disposal of {fixedAsset.Code} on {request.DisposedDate:yyyy-MM-dd}";
            var lines = new List<JournalEntryLineInput>();

            if (accumulatedDepreciation > 0m)
                lines.Add(new JournalEntryLineInput(accumulatedDepreciationAccountId!.Value, accumulatedDepreciation, 0m, reference, fixedAsset.CostCenterId));

            if (needsProceedsAccount)
                lines.Add(new JournalEntryLineInput(proceedsAccountId!.Value, request.DisposalProceeds, 0m, reference, fixedAsset.CostCenterId));

            if (gainOrLoss > 0m)
                lines.Add(new JournalEntryLineInput(gainLossAccountId!.Value, 0m, gainOrLoss, reference, fixedAsset.CostCenterId));
            else if (gainOrLoss < 0m)
                lines.Add(new JournalEntryLineInput(gainLossAccountId!.Value, -gainOrLoss, 0m, reference, fixedAsset.CostCenterId));

            lines.Add(new JournalEntryLineInput(fixedAsset.AssetAccountId, 0m, fixedAsset.AcquisitionCost, reference, fixedAsset.CostCenterId));

            // The lines above always balance by construction (see class doc comment's
            // worked-out equation), and AcquisitionCost > 0 (enforced at FixedAsset.Create)
            // guarantees at least one debit line exists alongside the asset credit line —
            // but guard defensively rather than let JournalEntry.Create's own "at least two
            // lines" check surface as an unexplained 500 if that invariant were ever violated.
            if (lines.Count >= 2)
            {
                var entry = JournalEntry.Create(request.CompanyId, reference, lines);
                entry.Post();
                await _journalEntryRepository.AddAsync(entry, cancellationToken);
                journalEntryDto = CreateJournalEntryCommandHandler.MapToDto(entry);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DisposeFixedAssetResultDto(
            CreateFixedAssetCommandHandler.MapToDto(fixedAsset),
            accumulatedDepreciation,
            netBookValue,
            gainOrLoss,
            journalEntryDto);
    }
}
