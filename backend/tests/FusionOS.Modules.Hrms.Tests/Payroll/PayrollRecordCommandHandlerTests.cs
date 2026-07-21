using FluentAssertions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.Payroll.Commands.ApprovePayrollRecord;
using FusionOS.Modules.Hrms.Application.Payroll.Commands.CreatePayrollDraft;
using FusionOS.Modules.Hrms.Application.Payroll.Commands.MarkPayrollRecordPaid;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.Payroll;

public class PayrollRecordCommandHandlerTests
{
    [Fact]
    public async Task CreatePayrollDraft_WhenEmployeeExistsAndNoDuplicate_PersistsDraft()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var repository = Substitute.For<IPayrollRecordRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, employeeId, Arg.Any<CancellationToken>()).Returns(true);
        repository.ExistsForPeriodAsync(companyId, employeeId, 6, 2024, Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePayrollDraftCommandHandler(repository, employeeRepository, unitOfWork);

        var result = await handler.Handle(
            new CreatePayrollDraftCommand(companyId, employeeId, 6, 2024, 5000m),
            CancellationToken.None);

        result.Status.Should().Be("Draft");
        result.GrossPay.Should().Be(5000m);
        await repository.Received(1).AddAsync(Arg.Any<Domain.Payroll.PayrollRecord>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatePayrollDraft_WhenEmployeeMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPayrollRecordRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreatePayrollDraftCommandHandler(repository, employeeRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreatePayrollDraftCommand(companyId, Guid.NewGuid(), 6, 2024, 5000m),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task CreatePayrollDraft_WhenDuplicateForPeriod_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var repository = Substitute.For<IPayrollRecordRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, employeeId, Arg.Any<CancellationToken>()).Returns(true);
        repository.ExistsForPeriodAsync(companyId, employeeId, 6, 2024, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreatePayrollDraftCommandHandler(repository, employeeRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreatePayrollDraftCommand(companyId, employeeId, 6, 2024, 5000m),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task ApprovePayrollRecord_ResolvesToApproved()
    {
        var companyId = Guid.NewGuid();
        var record = Domain.Payroll.PayrollRecord.CreateDraft(companyId, Guid.NewGuid(), 6, 2024, 5000m);
        var repository = Substitute.For<IPayrollRecordRepository>();
        repository.GetByIdAsync(companyId, record.Id, Arg.Any<CancellationToken>()).Returns(record);
        var handler = new ApprovePayrollRecordCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new ApprovePayrollRecordCommand(companyId, record.Id), CancellationToken.None);

        result.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task MarkPayrollRecordPaid_ResolvesToPaid()
    {
        var companyId = Guid.NewGuid();
        var record = Domain.Payroll.PayrollRecord.CreateDraft(companyId, Guid.NewGuid(), 6, 2024, 5000m);
        record.Approve(DateTimeOffset.UtcNow);
        var repository = Substitute.For<IPayrollRecordRepository>();
        repository.GetByIdAsync(companyId, record.Id, Arg.Any<CancellationToken>()).Returns(record);
        var handler = new MarkPayrollRecordPaidCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new MarkPayrollRecordPaidCommand(companyId, record.Id), CancellationToken.None);

        result.Status.Should().Be("Paid");
    }

    [Fact]
    public async Task MarkPayrollRecordPaid_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPayrollRecordRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.Payroll.PayrollRecord?)null);
        var handler = new MarkPayrollRecordPaidCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new MarkPayrollRecordPaidCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
