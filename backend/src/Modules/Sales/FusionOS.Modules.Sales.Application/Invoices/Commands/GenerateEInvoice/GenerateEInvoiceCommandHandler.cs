using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.Settings.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Invoices.Commands.GenerateEInvoice;

/// <summary>
/// GST e-invoicing (IRN/QR) — submits an Issued invoice to the GST Invoice Registration
/// Portal via <see cref="IGstIrpClient"/> and records the response on the invoice. Two
/// compliance rules enforced here, both already in force (not upcoming):
///
/// 1. Only companies with EInvoicingRequired set on GstComplianceSettings may e-invoice at
///    all — see GstComplianceSettings' doc comment for why this is an admin-set flag rather
///    than an auto-computed turnover check.
/// 2. The 1 Apr 2025 rule: an invoice more than 30 days old cannot be submitted to the IRP —
///    a stale submission is rejected by the real portal outright, so this handler rejects it
///    before ever calling IGstIrpClient, not after.
/// </summary>
public sealed class GenerateEInvoiceCommandHandler : IRequestHandler<GenerateEInvoiceCommand, InvoiceDto>
{
    private static readonly TimeSpan SubmissionWindow = TimeSpan.FromDays(30);

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IGstComplianceSettingsRepository _settingsRepository;
    private readonly IGstIrpClient _irpClient;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateEInvoiceCommandHandler(
        IInvoiceRepository invoiceRepository,
        IGstComplianceSettingsRepository settingsRepository,
        IGstIrpClient irpClient,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _settingsRepository = settingsRepository;
        _irpClient = irpClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> Handle(GenerateEInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.CompanyId, request.InvoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' was not found.");

        var settings = await _settingsRepository.GetByCompanyIdAsync(request.CompanyId, cancellationToken);
        if (settings is null || !settings.EInvoicingRequired)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CompanyId),
                    "E-invoicing is not enabled for this company — set EInvoicingRequired in GST compliance settings first."),
            });
        }

        var age = DateTimeOffset.UtcNow - invoice.InvoiceDate;
        if (age > SubmissionWindow)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.InvoiceId),
                    $"This invoice is {age.Days} days old — the GST IRP only accepts e-invoice submissions within 30 days of the invoice date (rule effective 1 Apr 2025)."),
            });
        }

        var payload = new GstIrpInvoicePayload(
            request.CompanyId,
            invoice.Id,
            invoice.InvoiceDate,
            invoice.TotalAmount,
            invoice.Lines.Select(l => new GstIrpInvoiceLine(l.ProductId, l.Quantity, l.UnitPrice, l.TaxAmount)).ToList());

        var result = await _irpClient.SubmitInvoiceAsync(payload, cancellationToken);

        invoice.RecordIrnGenerated(result.Irn, result.QrCodePayload, result.AckNumber, result.GeneratedAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateInvoiceCommandHandler.MapToDto(invoice);
    }
}
