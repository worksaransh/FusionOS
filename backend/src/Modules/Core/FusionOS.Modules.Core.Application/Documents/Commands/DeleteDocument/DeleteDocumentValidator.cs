using FluentValidation;

namespace FusionOS.Modules.Core.Application.Documents.Commands.DeleteDocument;

public sealed class DeleteDocumentValidator : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
