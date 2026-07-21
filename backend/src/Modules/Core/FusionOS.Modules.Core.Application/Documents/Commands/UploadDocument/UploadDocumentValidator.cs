using FluentValidation;

namespace FusionOS.Modules.Core.Application.Documents.Commands.UploadDocument;

public sealed class UploadDocumentValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.Content).NotEmpty().WithMessage("File content must not be empty.");
        RuleFor(x => x.Content)
            .Must(content => content.Length <= Domain.Documents.Document.MaxFileSizeBytes)
            .When(x => x.Content is { Length: > 0 })
            .WithMessage($"File exceeds the maximum allowed size of {Domain.Documents.Document.MaxFileSizeBytes / (1024 * 1024)} MB.");
    }
}
