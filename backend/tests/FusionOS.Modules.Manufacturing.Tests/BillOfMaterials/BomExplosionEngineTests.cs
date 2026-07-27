using FluentAssertions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ExplodeBillOfMaterials;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.BillOfMaterials;

/// <summary>
/// BomExplosionEngine itself is `internal static` (Application/BillOfMaterials/Contracts/BomExplosionEngine.cs)
/// with no InternalsVisibleTo anywhere in this repo, so these tests exercise it indirectly
/// through the public ExplodeBillOfMaterialsQueryHandler — the same recursion/cycle-guard code
/// runs either way (see ExplodeBillOfMaterialsQueryHandler.Handle, which calls
/// BomExplosionEngine.ExplodeAsync directly).
/// </summary>
public class BomExplosionEngineTests
{
    private static BillOfMaterials MakeBom(Guid companyId, Guid productId, string code, params BomLineInput[] lines) =>
        BillOfMaterials.Create(companyId, code, code, productId, lines);

    [Fact]
    public async Task Explode_CircularBom_DetectsCycleAndTreatsOccurrenceAsLeaf()
    {
        var companyId = Guid.NewGuid();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();

        // A contains 2x B; B (which has its own BOM) contains 3x A — a genuine cycle.
        var bomA = MakeBom(companyId, productA, "BOM-A", new BomLineInput(productB, 2m));
        var bomB = MakeBom(companyId, productB, "BOM-B", new BomLineInput(productA, 3m));

        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, bomA.Id, Arg.Any<CancellationToken>()).Returns(bomA);
        repository.GetPrimaryByProductIdAsync(companyId, productB, Arg.Any<CancellationToken>()).Returns(bomB);
        repository.GetPrimaryByProductIdAsync(companyId, productA, Arg.Any<CancellationToken>()).Returns(bomA);

        var handler = new ExplodeBillOfMaterialsQueryHandler(repository);

        var result = await handler.Handle(new ExplodeBillOfMaterialsQuery(companyId, bomA.Id, 1m), CancellationToken.None);

        // The cycle guard stops recursion (no stack overflow / infinite loop) and reports it.
        result.Warnings.Should().ContainSingle(w => w.Contains("Circular", StringComparison.OrdinalIgnoreCase));

        // Component B itself never becomes a leaf requirement (it has its own BOM, so it was
        // recursed into); the cycle is detected one level down, where B's line back to A is
        // treated as a leaf requirement instead of being exploded again: 3 (per B) * 2 (B needed
        // per A) * 1 (requested quantity of A) = 6.
        result.LeafRequirements.Should().ContainSingle();
        var leaf = result.LeafRequirements.Single();
        leaf.ProductId.Should().Be(productA);
        leaf.TotalQuantityRequired.Should().Be(6m);
    }

    [Fact]
    public async Task Explode_ThreeLevelChain_MultipliesQuantitiesDownEachLevel()
    {
        var companyId = Guid.NewGuid();
        var productRoot = Guid.NewGuid();
        var productMid = Guid.NewGuid();
        var productLeafParent = Guid.NewGuid();
        var leafComponent = Guid.NewGuid();

        // Root -> Mid (x2) -> LeafParent (x3) -> leafComponent (x5), a genuine 3-level-deep
        // manufactured chain (each of Root/Mid/LeafParent is itself manufactured, with its own
        // separate BillOfMaterials, per this module's deliberately-flat BOM design).
        var bomRoot = MakeBom(companyId, productRoot, "BOM-ROOT", new BomLineInput(productMid, 2m));
        var bomMid = MakeBom(companyId, productMid, "BOM-MID", new BomLineInput(productLeafParent, 3m));
        var bomLeafParent = MakeBom(companyId, productLeafParent, "BOM-LEAFPARENT", new BomLineInput(leafComponent, 5m));

        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, bomRoot.Id, Arg.Any<CancellationToken>()).Returns(bomRoot);
        repository.GetPrimaryByProductIdAsync(companyId, productMid, Arg.Any<CancellationToken>()).Returns(bomMid);
        repository.GetPrimaryByProductIdAsync(companyId, productLeafParent, Arg.Any<CancellationToken>()).Returns(bomLeafParent);
        repository.GetPrimaryByProductIdAsync(companyId, leafComponent, Arg.Any<CancellationToken>()).Returns((BillOfMaterials?)null);

        var handler = new ExplodeBillOfMaterialsQueryHandler(repository);

        // Building 10 units of the root product.
        var result = await handler.Handle(new ExplodeBillOfMaterialsQuery(companyId, bomRoot.Id, 10m), CancellationToken.None);

        result.Warnings.Should().BeEmpty();
        result.LeafRequirements.Should().ContainSingle();
        var leaf = result.LeafRequirements.Single();
        leaf.ProductId.Should().Be(leafComponent);
        // 10 (root qty) * 2 (Mid per Root) * 3 (LeafParent per Mid) * 5 (leaf per LeafParent) = 300.
        leaf.TotalQuantityRequired.Should().Be(300m);
    }

    [Fact]
    public async Task Explode_LeafAppearingInTwoBranches_AccumulatesTotalAcrossBranches()
    {
        var companyId = Guid.NewGuid();
        var productRoot = Guid.NewGuid();
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        var sharedLeaf = Guid.NewGuid();

        // Root needs 1x BranchA and 1x BranchB; both branches are themselves manufactured from
        // (different quantities of) the very same leaf component — a normal, non-cyclic case
        // that must still sum correctly across both branches.
        var bomRoot = MakeBom(companyId, productRoot, "BOM-ROOT",
            new BomLineInput(branchA, 1m), new BomLineInput(branchB, 1m));
        var bomBranchA = MakeBom(companyId, branchA, "BOM-BRANCH-A", new BomLineInput(sharedLeaf, 4m));
        var bomBranchB = MakeBom(companyId, branchB, "BOM-BRANCH-B", new BomLineInput(sharedLeaf, 6m));

        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, bomRoot.Id, Arg.Any<CancellationToken>()).Returns(bomRoot);
        repository.GetPrimaryByProductIdAsync(companyId, branchA, Arg.Any<CancellationToken>()).Returns(bomBranchA);
        repository.GetPrimaryByProductIdAsync(companyId, branchB, Arg.Any<CancellationToken>()).Returns(bomBranchB);
        repository.GetPrimaryByProductIdAsync(companyId, sharedLeaf, Arg.Any<CancellationToken>()).Returns((BillOfMaterials?)null);

        var handler = new ExplodeBillOfMaterialsQueryHandler(repository);

        var result = await handler.Handle(new ExplodeBillOfMaterialsQuery(companyId, bomRoot.Id, 1m), CancellationToken.None);

        result.Warnings.Should().BeEmpty();
        result.LeafRequirements.Should().ContainSingle();
        result.LeafRequirements.Single().ProductId.Should().Be(sharedLeaf);
        result.LeafRequirements.Single().TotalQuantityRequired.Should().Be(10m); // 4 (via A) + 6 (via B)
    }

    [Fact]
    public async Task Explode_DeepNonCyclicChain_TruncatesAtMaxDepthWithWarningInsteadOfOverflowing()
    {
        var companyId = Guid.NewGuid();

        // A straight-line chain of 25 distinct manufactured products (no cycle — each is a
        // brand new Guid), deliberately deeper than BomExplosionEngine.MaxRecursionDepth (20),
        // to confirm the depth cap truncates gracefully with a warning rather than the engine
        // recursing forever (it wouldn't, since this isn't cyclic — the point is that a very
        // deep but valid chain is still bounded rather than walked indefinitely).
        const int chainLength = 25;
        var productIds = Enumerable.Range(0, chainLength).Select(_ => Guid.NewGuid()).ToArray();
        var boms = new BillOfMaterials[chainLength - 1];
        for (var i = 0; i < chainLength - 1; i++)
        {
            boms[i] = MakeBom(companyId, productIds[i], $"BOM-{i}", new BomLineInput(productIds[i + 1], 1m));
        }

        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.GetByIdAsync(companyId, boms[0].Id, Arg.Any<CancellationToken>()).Returns(boms[0]);
        for (var i = 0; i < chainLength - 1; i++)
        {
            var nextBom = i + 1 < boms.Length ? boms[i + 1] : null;
            repository.GetPrimaryByProductIdAsync(companyId, productIds[i + 1], Arg.Any<CancellationToken>()).Returns(nextBom);
        }

        var handler = new ExplodeBillOfMaterialsQueryHandler(repository);

        var result = await handler.Handle(new ExplodeBillOfMaterialsQuery(companyId, boms[0].Id, 1m), CancellationToken.None);

        result.Warnings.Should().ContainSingle(w => w.Contains("maximum supported explosion depth", StringComparison.OrdinalIgnoreCase));
    }
}
