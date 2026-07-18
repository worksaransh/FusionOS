using FluentAssertions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.ApproveLeaveRequest;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.CreateLeaveRequest;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.RejectLeaveRequest;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.LeaveRequests;

public class LeaveRequestCommandHandlerTests
{
    private static readonly DateTimeOffset Start = new(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2024, 6, 5, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateLeaveRequest_WhenEmployeeExists_PersistsRequestedRequest()
    {
        var companyId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var repository = Substitute.For<ILeaveRequestRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, employeeId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateLeaveRequestCommandHandler(repository, employeeRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateLeaveRequestCommand(companyId, employeeId, LeaveType.Annual, Start, End, "Family trip"),
            CancellationToken.None);

        result.Status.Should().Be("Requested");
        await repository.Received(1).AddAsync(Arg.Any<Domain.LeaveRequests.LeaveRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateLeaveRequest_WhenEmployeeMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ILeaveRequestRepository>();
        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateLeaveRequestCommandHandler(repository, employeeRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateLeaveRequestCommand(companyId, Guid.NewGuid(), LeaveType.Sick, Start, End, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task ApproveLeaveRequest_ResolvesToApproved()
    {
        var companyId = Guid.NewGuid();
        var request = Domain.LeaveRequests.LeaveRequest.Create(companyId, Guid.NewGuid(), LeaveType.Annual, Start, End, null);
        var repository = Substitute.For<ILeaveRequestRepository>();
        repository.GetByIdAsync(companyId, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var handler = new ApproveLeaveRequestCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new ApproveLeaveRequestCommand(companyId, request.Id), CancellationToken.None);

        result.Status.Should().Be("Approved");
    }

    [Fact]
    public async Task RejectLeaveRequest_ResolvesToRejected()
    {
        var companyId = Guid.NewGuid();
        var request = Domain.LeaveRequests.LeaveRequest.Create(companyId, Guid.NewGuid(), LeaveType.Annual, Start, End, null);
        var repository = Substitute.For<ILeaveRequestRepository>();
        repository.GetByIdAsync(companyId, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var handler = new RejectLeaveRequestCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new RejectLeaveRequestCommand(companyId, request.Id), CancellationToken.None);

        result.Status.Should().Be("Rejected");
    }
}
