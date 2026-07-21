using FluentAssertions;
using FusionOS.Modules.Hrms.Application.Attendance.Commands.RecordAttendance;
using FusionOS.Modules.Hrms.Application.Attendance.Commands.UpdateAttendance;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using FusionOS.Modules.Hrms.Domain.Attendance;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.Attendance;

public class AttendanceRecordCommandHandlerTests
{
    private static readonly DateTimeOffset Date = new(2024, 6, 3, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RecordAttendance_WhenEmployeeExists_PersistsRecord()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var repository = Substitute.For<IAttendanceRecordRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, employeeId, Arg.Any<CancellationToken>()).Returns(true);
        var leaveRequestRepository = Substitute.For<ILeaveRequestRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordAttendanceCommandHandler(repository, employeeRepository, leaveRequestRepository, unitOfWork);

        var result = await handler.Handle(
            new RecordAttendanceCommand(companyId, employeeId, Date, null, null, AttendanceStatus.Present, null),
            CancellationToken.None);

        result.Status.Should().Be("Present");
        await repository.Received(1).AddAsync(Arg.Any<Domain.Attendance.AttendanceRecord>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordAttendance_WhenEmployeeMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IAttendanceRecordRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var leaveRequestRepository = Substitute.For<ILeaveRequestRepository>();
        var handler = new RecordAttendanceCommandHandler(repository, employeeRepository, leaveRequestRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new RecordAttendanceCommand(companyId, Guid.NewGuid(), Date, null, null, AttendanceStatus.Absent, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task RecordAttendance_WhenLeaveRequestMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var repository = Substitute.For<IAttendanceRecordRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, employeeId, Arg.Any<CancellationToken>()).Returns(true);
        var leaveRequestRepository = Substitute.For<ILeaveRequestRepository>();
        leaveRequestRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.LeaveRequests.LeaveRequest?)null);
        var handler = new RecordAttendanceCommandHandler(repository, employeeRepository, leaveRequestRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new RecordAttendanceCommand(companyId, employeeId, Date, null, null, AttendanceStatus.OnLeave, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task UpdateAttendance_WhenRecordExists_UpdatesFields()
    {
        var companyId = Guid.NewGuid();
        var record = Domain.Attendance.AttendanceRecord.Create(companyId, Guid.NewGuid(), Date, null, null, AttendanceStatus.Absent, null);
        var repository = Substitute.For<IAttendanceRecordRepository>();
        repository.GetByIdAsync(companyId, record.Id, Arg.Any<CancellationToken>()).Returns(record);
        var leaveRequestRepository = Substitute.For<ILeaveRequestRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateAttendanceCommandHandler(repository, leaveRequestRepository, unitOfWork);

        var result = await handler.Handle(
            new UpdateAttendanceCommand(companyId, record.Id, null, null, AttendanceStatus.Present, null),
            CancellationToken.None);

        result.Status.Should().Be("Present");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAttendance_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IAttendanceRecordRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.Attendance.AttendanceRecord?)null);
        var handler = new UpdateAttendanceCommandHandler(repository, Substitute.For<ILeaveRequestRepository>(), Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new UpdateAttendanceCommand(companyId, Guid.NewGuid(), null, null, AttendanceStatus.Present, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
