using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateBillOfMaterials;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Manufacturing.Tests.BillOfMaterials;

public class CreateBillOfMaterialsCommandHandlerTests
{
    private static CreateBillOfMaterialsCommand Command(Guid companyId) =>
        new(companyId, "widget-a", "Widget A", Guid.NewGuid(), new[] { new BomLineInput(Guid.NewGuid(), 2m) });

    [Fact]
    public async Task Handle_CreatesBom_WhenCodeIsUnique()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.CodeExistsAsync(companyId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBillOfMaterialsCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(Command(companyId), CancellationToken.None);

        result.Code.Should().Be("WIDGET-A");
        result.Lines.Should().ContainSingle();
        await repository.Received(1).AddAsync(Arg.Any<Domain.BillOfMaterials.BillOfMaterials>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProductHasNoExistingActiveBom_CreatesAsPrimary()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.CodeExistsAsync(companyId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        repository.ListActiveByProductIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Domain.BillOfMaterials.BillOfMaterials>());
        var handler = new CreateBillOfMaterialsCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(Command(companyId), CancellationToken.None);

        result.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenProductAlreadyHasAnActiveBom_CreatesAsNonPrimaryAlternative()
    {
        // Alternative BOM (Phase 6, 2026-07-22) — a second BOM for a product that already
        // has one must NOT also default to primary, or two rows end up IsPrimary=true.
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var command = new CreateBillOfMaterialsCommand(companyId, "widget-b", "Widget B", productId, new[] { new BomLineInput(Guid.NewGuid(), 2m) });
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.CodeExistsAsync(companyId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var existingBom = Domain.BillOfMaterials.BillOfMaterials.Create(companyId, "widget-a", "Widget A", productId, new[] { new BomLineInput(Guid.NewGuid(), 1m) });
        repository.ListActiveByProductIdAsync(companyId, productId, Arg.Any<CancellationToken>())
            .Returns(new[] { existingBom });
        var handler = new CreateBillOfMaterialsCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_ThrowsValidationException()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IBillOfMaterialsRepository>();
        repository.CodeExistsAsync(companyId, Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateBillOfMaterialsCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(Command(companyId), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
