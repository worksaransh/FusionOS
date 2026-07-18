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
