namespace FusionOS.Modules.Sales.Domain.Returns;

/// <summary>
/// Per-line data captured only at Complete() time — the condition of the physically
/// returned goods and whether they were restocked can only be truly known once the
/// goods have arrived back and been inspected, which is why these are not supplied at
/// request time (see SalesReturnLineInput.ConditionNotes, which is only a customer's or
/// requester's initial, pre-inspection description).
/// </summary>
public sealed record SalesReturnLineCompletion(Guid LineId, bool Restocked, string? ConditionNotes);
