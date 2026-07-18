using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.CreateJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.PostMonthlyDepreciation;

public sealed class PostMonthlyDepreciationCommandHandler : IRequestHandler<PostMonthlyDepreciationCommand, JournalEntryDto>
{
    private readonly IFixedAssetRepository _fixedAssetRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PostMonthlyDepreciationCommandHandler(
        IFixedAssetRepository fixedAssetRepository,
        IAccountRepository accountRepository,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _fixedAssetRepository = fixedAssetRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntryDto> Handle(PostMonthlyDepreciationCommand request, CancellationToken cancellationToken)
    {
        var asset = await _fixedAssetRepository.GetByIdAsync(request.CompanyId, request.FixedAssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Fixed asset '{request.FixedAssetId}' was not found.");

        if (asset.IsDisposed)
            throw Validation(nameof(request.FixedAssetId), "Cannot post depreciation for a disposed fixed asset.");

        if (asset.AccumulatedDepreciationAccountId is not { } accumulatedDepreciationAccountId)
            throw Validation(nameof(asset.AccumulatedDepreciationAccountId),
                "This fixed asset has no accumulated-depreciation account set, so depreciation cannot be posted to the ledger.");

        if (!await _accountRepository.ExistsAsync(request.CompanyId, request.DepreciationExpenseAccountId, cancellationToken))
            throw Validation(nameof(request.DepreciationExpenseAccountId),
                $"Depreciation expense account '{request.DepreciationExpenseAccountId}' does not exist for this company.");

        if (!await _accountRepository.ExistsAsync(request.CompanyId, accumulatedDepreciationAccountId, cancellationToken))
            throw Validation(nameof(asset.AccumulatedDepreciationAccountId),
                $"Accumulated depreciation account '{accumulatedDepreciationAccountId}' does not exist for this company.");

        // Same straight-line monthly figure GetDepreciationScheduleQueryHandler computes,
        // rounded to the numeric(19,4) scale the ledger persists at.
        var monthlyAmount = Math.Round(
            (asset.AcquisitionCost - asset.SalvageValue) / asset.UsefulLifeMonths, 4, MidpointRounding.AwayFromZero);

        if (monthlyAmount <= 0)
            throw Validation(nameof(request.FixedAssetId), "Computed monthly depreciation amount is not positive; nothing to post.");

        var reference = $"Depreciation for {asset.Code} period ending {request.PeriodEnd:yyyy-MM-dd}";

        // Debit Depreciation Expense (tagged with the asset's cost center, if any, so the
        // posting flows into cost-center-aware Budget vs-actual) / Credit Accumulated
        // Depreciation — a balanced two-line entry, immediately posted to affect the GL.
        var lines = new[]
        {
            new JournalEntryLineInput(request.DepreciationExpenseAccountId, monthlyAmount, 0m, reference, asset.CostCenterId),
            new JournalEntryLineInput(accumulatedDepreciationAccountId, 0m, monthlyAmount, reference),
        };

        var entry = JournalEntry.Create(request.CompanyId, reference, lines);
        entry.Post();

        await _journalEntryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateJournalEntryCommandHandler.MapToDto(entry);
    }

    private static ValidationException Validation(string property, string message) =>
        new(new[] { new FluentValidation.Results.ValidationFailure(property, message) });
}
