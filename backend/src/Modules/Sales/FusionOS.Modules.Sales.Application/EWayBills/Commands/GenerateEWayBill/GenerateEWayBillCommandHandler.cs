using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Application.EWayBills.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.EWayBills.Commands.GenerateEWayBill;

/// <summary>
/// GST e-way bill generation — submits a Dispatch to the (sandboxed, see
/// Infrastructure/Gst/SandboxEWayBillClient) e-way bill API and records the result. Enforces
/// the 1 Jan 2025 rule: an e-way bill (or its Part-A slip) cannot be generated or updated for
/// a dispatch older than 180 days — checked before ever calling IEWayBillClient, same
/// "reject before calling the external system" shape as GenerateEInvoiceCommandHandler's
/// 30-day check.
/// </summary>
public sealed class GenerateEWayBillCommandHandler : IRequestHandler<GenerateEWayBillCommand, EWayBillDto>
{
    private static readonly TimeSpan GenerationWindow = TimeSpan.FromDays(180);

    private readonly IDispatchRepository _dispatchRepository;
    private readonly IEWayBillRepository _eWayBillRepository;
    private readonly IEWayBillClient _client;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateEWayBillCommandHandler(
        IDispatchRepository dispatchRepository,
        IEWayBillRepository eWayBillRepository,
        IEWayBillClient client,
        IUnitOfWork unitOfWork)
    {
        _dispatchRepository = dispatchRepository;
        _eWayBillRepository = eWayBillRepository;
        _client = client;
        _unitOfWork = unitOfWork;
    }

    public async Task<EWayBillDto> Handle(GenerateEWayBillCommand request, CancellationToken cancellationToken)
    {
        var dispatch = await _dispatchRepository.GetByIdAsync(request.CompanyId, request.DispatchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Dispatch '{request.DispatchId}' was not found.");

        var existing = await _eWayBillRepository.GetByDispatchIdAsync(request.CompanyId, request.DispatchId, cancellationToken);
        if (existing is not null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.DispatchId), "This dispatch already has an e-way bill."),
            });
        }

        var age = DateTimeOffset.UtcNow - dispatch.DispatchDate;
        if (age > GenerationWindow)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.DispatchId),
                    $"This dispatch is {age.Days} days old — an e-way bill cannot be generated for a dispatch older than 180 days (rule effective 1 Jan 2025)."),
            });
        }

        var result = await _client.GenerateAsync(new EWayBillGenerationPayload(request.CompanyId, request.DispatchId, dispatch.DispatchDate), cancellationToken);

        var eWayBill = Domain.EWayBills.EWayBill.Create(request.CompanyId, request.DispatchId, result.EwbNumber, result.GeneratedAt, result.ValidUntil);
        await _eWayBillRepository.AddAsync(eWayBill, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EWayBillDto(eWayBill.Id, eWayBill.DispatchId, eWayBill.EwbNumber, eWayBill.GeneratedAt, eWayBill.ValidUntil);
    }
}
