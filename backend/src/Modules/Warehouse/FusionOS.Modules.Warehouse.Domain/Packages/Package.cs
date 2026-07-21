using FusionOS.SharedKernel;

namespace FusionOS.Modules.Warehouse.Domain.Packages;

/// <summary>
/// Packing depth pass (2026-07-21 follow-up to Phase M9's Picking+Packing slice, PickList.cs).
/// PickList.Pack() only ever flips <see cref="PickLists.PickListStatus"/> to Packed — it never
/// captured WHICH items went into WHICH physical carton, nor any package weight/dimensions.
/// Package is that missing detail, recorded as its own aggregate alongside PickList, not a
/// replacement for the status flag: PickList.Pack()'s existing behavior is unchanged, and other
/// code that only cares "has this list been packed" keeps reading PickListStatus.Packed exactly as
/// before.
///
/// PickListId is a same-module reference to PickList — a real FK, but existence AND status
/// (must already be Packed) are validated by the command handler, not here, mirroring how BinId is
/// validated by CreatePickListCommandHandler rather than inside PickList itself: this aggregate has
/// no repository access of its own to check either.
///
/// PackageNumber is caller-supplied (not auto-sequenced by this aggregate) and must be unique per
/// PickList — enforced by the command handler via IPackageRepository.PackageNumberExistsAsync, same
/// "handler checks uniqueness before Create" shape as Bin/Zone's CodeExistsAsync checks.
///
/// ProductId on each <see cref="PackageLine"/> is an opaque cross-module reference into Inventory's
/// Product aggregate, unvalidated — same convention as PickListLine.ProductId
/// (03_SYSTEM_ARCHITECTURE.md §2).
/// </summary>
public sealed class Package : TenantAggregateRoot
{
    private readonly List<PackageLine> _lines = new();

    public Guid PickListId { get; private set; }
    public string PackageNumber { get; private set; } = default!;
    public decimal? WeightKg { get; private set; }
    public decimal? LengthCm { get; private set; }
    public decimal? WidthCm { get; private set; }
    public decimal? HeightCm { get; private set; }
    public IReadOnlyList<PackageLine> Lines => _lines.AsReadOnly();

    private Package() { }

    public static Package Create(
        Guid companyId,
        Guid pickListId,
        string packageNumber,
        decimal? weightKg,
        decimal? lengthCm,
        decimal? widthCm,
        decimal? heightCm,
        IReadOnlyCollection<PackageLineInput> lines)
    {
        if (pickListId == Guid.Empty)
            throw new ArgumentException("Pick list id is required.", nameof(pickListId));
        if (string.IsNullOrWhiteSpace(packageNumber))
            throw new ArgumentException("Package number is required.", nameof(packageNumber));
        if (packageNumber.Length > 50)
            throw new ArgumentException("Package number cannot exceed 50 characters.", nameof(packageNumber));
        if (weightKg is < 0)
            throw new ArgumentException("Weight cannot be negative.", nameof(weightKg));
        if (lengthCm is < 0)
            throw new ArgumentException("Length cannot be negative.", nameof(lengthCm));
        if (widthCm is < 0)
            throw new ArgumentException("Width cannot be negative.", nameof(widthCm));
        if (heightCm is < 0)
            throw new ArgumentException("Height cannot be negative.", nameof(heightCm));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A package must have at least one line.", nameof(lines));

        var package = new Package
        {
            CompanyId = companyId,
            PickListId = pickListId,
            PackageNumber = packageNumber.Trim(),
            WeightKg = weightKg,
            LengthCm = lengthCm,
            WidthCm = widthCm,
            HeightCm = heightCm,
        };

        foreach (var line in lines)
            package._lines.Add(PackageLine.Create(line.ProductId, line.Quantity));

        return package;
    }
}
