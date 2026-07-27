namespace FusionOS.Modules.Hrms.Domain.Onboarding;

/// <summary>Distinguishes an Onboarding workflow from an Offboarding one — see EmployeeWorkflow's class doc comment for why both share this one entity.</summary>
public enum WorkflowKind
{
    Onboarding = 0,
    Offboarding = 1,
}
