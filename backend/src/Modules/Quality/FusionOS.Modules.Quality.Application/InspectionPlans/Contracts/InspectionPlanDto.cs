namespace FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;

public sealed record InspectionPlanCharacteristicDto(Guid Id, string CharacteristicName, int SequenceNumber);

public sealed record InspectionPlanDto(
    Guid Id,
    string Name,
    string InspectionType,
    bool IsActive,
    IReadOnlyList<InspectionPlanCharacteristicDto> Characteristics);

/// <summary>Single place that turns an InspectionPlan aggregate into its DTO, shared by every handler that returns one.</summary>
public static class InspectionPlanMapper
{
    public static InspectionPlanDto ToDto(Domain.InspectionPlans.InspectionPlan plan) => new(
        plan.Id,
        plan.Name,
        plan.InspectionType.ToString(),
        plan.IsActive,
        plan.Characteristics
            .OrderBy(c => c.SequenceNumber)
            .Select(c => new InspectionPlanCharacteristicDto(c.Id, c.CharacteristicName, c.SequenceNumber))
            .ToList());
}
