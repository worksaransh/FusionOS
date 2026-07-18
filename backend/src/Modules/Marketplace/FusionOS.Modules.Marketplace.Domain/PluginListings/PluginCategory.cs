namespace FusionOS.Modules.Marketplace.Domain.PluginListings;

/// <summary>The six categories named in 05_MODULE_ROADMAP.md's Marketplace line item. Stored as text via EF value conversion, never a native PostgreSQL enum (04_DATABASE_GUIDELINES.md §10).</summary>
public enum PluginCategory
{
    Plugin,
    Theme,
    ReportPack,
    WorkflowPack,
    IndustryExtension,
    AiAgent,
}
