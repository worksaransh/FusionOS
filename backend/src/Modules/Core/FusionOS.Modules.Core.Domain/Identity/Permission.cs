using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Identity;

/// <summary>Global reference data, e.g. "procurement.purchase-order.approve" — seeded, not user-created.</summary>
public sealed class Permission : AuditableEntity
{
    public string Code { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string Module { get; private set; } = default!;

    private Permission() { }

    public static Permission Create(string module, string code, string description) => new()
    {
        Module = module,
        Code = code,
        Description = description,
    };
}
