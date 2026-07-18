using FluentValidation;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;

public sealed class CreateMaintenanceRequestValidator : AbstractValidator<CreateMaintenanceRequestCommand>
{
    public CreateMaintenanceRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
    }
}
