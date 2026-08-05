using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Invoices.Commands.GenerateEInvoice;

public sealed class GenerateEInvoiceValidator : AbstractValidator<GenerateEInvoiceCommand>
{
    public GenerateEInvoiceValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.InvoiceId).NotEmpty();
    }
}
